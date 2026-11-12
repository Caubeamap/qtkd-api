using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QTKD.Api.DTOs;
using QTKD.Api.Entities;
using Xunit;

namespace QTKD.Api.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class WorkflowTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;
    private readonly List<int> _customerIds = [];
    private readonly List<int> _contractIds = [];
    private readonly List<int> _priceListIds = [];

    public WorkflowTests(ApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.NewContext();
        await db.Contracts.Where(c => _contractIds.Contains(c.Id)).ExecuteDeleteAsync();
        await db.Customers.Where(c => _customerIds.Contains(c.Id)).ExecuteDeleteAsync();
        await db.PriceItems.Where(p => _priceListIds.Contains(p.PriceListId)).ExecuteDeleteAsync();
        await db.PriceLists.Where(p => _priceListIds.Contains(p.Id)).ExecuteDeleteAsync();
        _client.Dispose();
    }

    private async Task<CustomerResponseDto> Customer()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", new { Name = $"Test-{Guid.NewGuid():N}", Status = "Active" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = (await response.Content.ReadFromJsonAsync<CustomerResponseDto>())!;
        _customerIds.Add(result.Id);
        return result;
    }

    private static ContractRequestDto Request(int customerId) => new()
    {
        ContractNo = $"TEST-{Guid.NewGuid():N}", CustomerId = customerId,
        StartDate = new(2026, 11, 1), EndDate = new(2027, 11, 1),
        Content = "Service contract", DocumentName = "contract.pdf"
    };

    private async Task<ContractResponseDto> Create(ContractRequestDto request)
    {
        var response = await _client.PostAsJsonAsync("/api/contracts", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = (await response.Content.ReadFromJsonAsync<ContractResponseDto>())!;
        _contractIds.Add(result.Id);
        return result;
    }

    private async Task<Contract> Row(int id)
    {
        await using var db = _factory.NewContext();
        return await db.Contracts.AsNoTracking().SingleAsync(c => c.Id == id);
    }

    // Every scalar field is compared; navigation properties are deliberately excluded.
    private static void Same(Contract expected, Contract actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.ContractNo, actual.ContractNo);
        Assert.Equal(expected.CustomerId, actual.CustomerId);
        Assert.Equal(expected.StartDate, actual.StartDate);
        Assert.Equal(expected.EndDate, actual.EndDate);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.Content, actual.Content);
        Assert.Equal(expected.DocumentName, actual.DocumentName);
    }

    private async Task Verify(int id, ContractRequestDto expected, string status)
    {
        var get = await _client.GetAsync($"/api/contracts/{id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var body = (await get.Content.ReadFromJsonAsync<ContractResponseDto>())!;
        var row = await Row(id);
        Same(new Contract { Id = id, ContractNo = expected.ContractNo, CustomerId = expected.CustomerId,
            StartDate = expected.StartDate, EndDate = expected.EndDate, Content = expected.Content ?? "",
            DocumentName = expected.DocumentName ?? "", Status = status }, row);
        Assert.Equal(id, body.Id);
        Assert.Equal(row.ContractNo, body.ContractNo);
        Assert.Equal(row.CustomerId, body.CustomerId);
        Assert.Equal(row.StartDate, body.StartDate);
        Assert.Equal(row.EndDate, body.EndDate);
        Assert.Equal(row.Content, body.Content);
        Assert.Equal(row.DocumentName, body.DocumentName);
        Assert.Equal(status, body.Status);
    }

    private static async Task Problem(HttpResponseMessage response, HttpStatusCode status, string detail)
    {
        Assert.Equal(status, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal((int)status, json.GetProperty("status").GetInt32());
        Assert.Contains(detail, json.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TC01_CreateCustomer_ReturnsLocationBodyGetAndDatabase()
    {
        var name = $"Test-{Guid.NewGuid():N}";
        var response = await _client.PostAsJsonAsync("/api/customers", new { Name = name, Status = "Active" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = (await response.Content.ReadFromJsonAsync<CustomerResponseDto>())!;
        _customerIds.Add(body.Id);
        Assert.True(body.Id > 0);
        Assert.EndsWith($"/api/customers/{body.Id}", response.Headers.Location!.ToString());
        Assert.Equal(name, body.Name);
        Assert.Equal("Active", body.Status);
        var get = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = (await get.Content.ReadFromJsonAsync<CustomerResponseDto>())!;
        Assert.Equal(body.Id, read.Id);
        Assert.Equal(name, read.Name);
        Assert.Equal("Active", read.Status);
        await using var db = _factory.NewContext();
        var row = await db.Customers.AsNoTracking().SingleAsync(c => c.Id == body.Id);
        Assert.Equal(name, row.Name);
        Assert.Equal("Active", row.Status);
    }

    [Theory]
    [InlineData("{\"status\":\"Active\"}", "Name")]
    [InlineData("{\"name\":null,\"status\":\"Active\"}", "Name")]
    [InlineData("{\"name\":\"   \",\"status\":\"Active\"}", "Name")]
    [InlineData("{\"name\":\"Test\"}", "Status")]
    [InlineData("{\"name\":\"Test\",\"status\":null}", "Status")]
    [InlineData("{\"name\":\"Test\",\"status\":\"   \"}", "Status")]
    public async Task TC02_InvalidCustomer_ReturnsValidationErrorsAndNoRow(string json, string field)
    {
        await using var before = _factory.NewContext();
        var count = await before.Customers.AsNoTracking().CountAsync();
        var response = await _client.PostAsync("/api/customers", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").GetProperty(field).GetArrayLength() > 0);
        await using var after = _factory.NewContext();
        Assert.Equal(count, await after.Customers.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task TC03_UpdateDraft_PersistsExactFields()
    {
        var request = Request((await Customer()).Id);
        var contract = await Create(request);
        var update = Request((await Customer()).Id);
        update.StartDate = new(2026, 12, 2);
        update.EndDate = new(2028, 1, 3);
        update.Content = "Updated content";
        update.DocumentName = "updated.pdf";
        var response = await _client.PutAsJsonAsync($"/api/contracts/{contract.Id}", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = (await response.Content.ReadFromJsonAsync<ContractResponseDto>())!;
        Assert.Equal(contract.Id, body.Id);
        Assert.Equal(update.ContractNo, body.ContractNo);
        Assert.Equal(update.CustomerId, body.CustomerId);
        Assert.Equal(update.StartDate, body.StartDate);
        Assert.Equal(update.EndDate, body.EndDate);
        Assert.Equal(update.Content, body.Content);
        Assert.Equal(update.DocumentName, body.DocumentName);
        Assert.Equal("Draft", body.Status);
        await Verify(contract.Id, update, "Draft");
    }

    [Fact]
    public async Task TC04_UpdatePendingApproval_ReturnsConflictAndPreservesAllFields()
    {
        var request = Request((await Customer()).Id);
        var contract = await Create(request);
        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null)).StatusCode);
        var snapshot = await Row(contract.Id);
        await Problem(await _client.PutAsJsonAsync($"/api/contracts/{contract.Id}", Request((await Customer()).Id)), HttpStatusCode.Conflict, "CTR-01");
        Same(snapshot, await Row(contract.Id));
    }

    [Theory]
    [InlineData(true, "Contract content is required.")]
    [InlineData(false, "A required document is missing.")]
    public async Task TC05_SubmitMissingCondition_ReturnsCtr02AndPreservesDraft(bool content, string error)
    {
        var request = Request((await Customer()).Id);
        if (content) request.Content = ""; else request.DocumentName = "";
        var contract = await Create(request);
        var snapshot = await Row(contract.Id);
        var response = await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("CTR-02", body.GetProperty("code").GetString());
        Assert.Equal(new[] { error }, body.GetProperty("errors").EnumerateArray().Select(e => e.GetString()));
        Same(snapshot, await Row(contract.Id));
        await Verify(contract.Id, request, "Draft");
    }

    private async Task Submit(int id)
    {
        var response = await _client.PostAsync($"/api/contracts/{id}/submit", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = (await response.Content.ReadFromJsonAsync<SubmitContractResponseDto>())!;
        Assert.Equal(id, body.ContractId);
        Assert.Equal("PendingApproval", body.Status);
        Assert.Equal("Contract submitted for approval.", body.Message);
    }

    [Fact]
    public async Task TC06_SubmitValid_PersistsPendingApprovalOnly()
    {
        var request = Request((await Customer()).Id);
        var contract = await Create(request);
        var snapshot = await Row(contract.Id);
        await Submit(contract.Id);
        snapshot.Status = "PendingApproval";
        Same(snapshot, await Row(contract.Id));
        await Verify(contract.Id, request, "PendingApproval");
    }

    [Fact]
    public async Task TC07_RawApprovedStatus_Returns400AndPreservesAllFields()
    {
        var request = Request((await Customer()).Id);
        var contract = await Create(request);
        var snapshot = await Row(contract.Id);
        var response = await _client.PutAsJsonAsync($"/api/contracts/{contract.Id}", new {
            ContractNo = "Changed-by-invalid-request", request.CustomerId, request.StartDate,
            request.EndDate, request.Content, request.DocumentName, Status = "Approved" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(body.GetProperty("errors").EnumerateObject(),
            error => error.Name.Contains("status", StringComparison.OrdinalIgnoreCase));
        Same(snapshot, await Row(contract.Id));
    }

    [Fact]
    public async Task TC08_SequentialDuplicateSubmit_ReturnsConflictWithoutNewContract()
    {
        var contract = await Create(Request((await Customer()).Id));
        await Submit(contract.Id);
        var snapshot = await Row(contract.Id);
        await using var before = _factory.NewContext();
        var count = await before.Contracts.AsNoTracking().CountAsync();
        await Problem(await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null), HttpStatusCode.Conflict, "must be in Draft");
        Same(snapshot, await Row(contract.Id));
        await using var after = _factory.NewContext();
        Assert.Equal(count, await after.Contracts.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task Week06_EndToEnd_MissingDraftThenCompleteAndSubmit()
    {
        var request = Request((await Customer()).Id);
        request.Content = "";
        request.DocumentName = "";
        var contract = await Create(request);
        var response = await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("CTR-02", errors.GetProperty("code").GetString());
        Assert.Equal(2, errors.GetProperty("errors").GetArrayLength());
        await Verify(contract.Id, request, "Draft");
        request.Content = "Completed content";
        request.DocumentName = "complete.pdf";
        Assert.Equal(HttpStatusCode.OK, (await _client.PutAsJsonAsync($"/api/contracts/{contract.Id}", request)).StatusCode);
        await Submit(contract.Id);
        await Verify(contract.Id, request, "PendingApproval");
    }

    [Fact]
    public async Task Pricing_ValidItemsPersist_InvalidPriceMissingListAndDuplicateRejected()
    {
        var name = $"Test-{Guid.NewGuid():N}";
        var date = new DateOnly(2026, 11, 1);
        var created = await _client.PostAsJsonAsync("/api/price-lists", new { Name = name, EffectiveDate = date });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var list = (await created.Content.ReadFromJsonAsync<PriceListResponseDto>())!;
        _priceListIds.Add(list.Id);
        Assert.Equal(name, list.Name);
        Assert.Equal(date, list.EffectiveDate);
        var path = $"/api/price-lists/{list.Id}/items";
        var added = await _client.PostAsJsonAsync(path, new { ServiceCode = "TEST-SVC", UnitPrice = 123.45m });
        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        var item = (await added.Content.ReadFromJsonAsync<PriceItemResponseDto>())!;
        Assert.Equal(list.Id, item.PriceListId);
        Assert.Equal("TEST-SVC", item.ServiceCode);
        Assert.Equal(123.45m, item.UnitPrice);
        var get = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Single((await get.Content.ReadFromJsonAsync<List<PriceItemResponseDto>>())!);
        var invalid = await _client.PostAsJsonAsync(path, new { ServiceCode = "INVALID", UnitPrice = 0m });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var errors = await invalid.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(errors.GetProperty("errors").GetProperty("UnitPrice").GetArrayLength() > 0);
        await Problem(await _client.PostAsJsonAsync("/api/price-lists/2147483647/items", new { ServiceCode = "TEST-SVC", UnitPrice = 1m }), HttpStatusCode.NotFound, "not found");
        await Problem(await _client.PostAsJsonAsync(path, new { ServiceCode = "TEST-SVC", UnitPrice = 2m }), HttpStatusCode.Conflict, "already");
        await using var db = _factory.NewContext();
        var row = await db.PriceLists.AsNoTracking().SingleAsync(p => p.Id == list.Id);
        Assert.Equal(name, row.Name);
        Assert.Equal(date, row.EffectiveDate);
        var persisted = await db.PriceItems.AsNoTracking().SingleAsync(p => p.PriceListId == list.Id);
        Assert.Equal("TEST-SVC", persisted.ServiceCode);
        Assert.Equal(123.45m, persisted.UnitPrice);
    }
}
