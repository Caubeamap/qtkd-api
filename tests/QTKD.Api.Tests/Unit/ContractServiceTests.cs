using QTKD.Api.DTOs;
using QTKD.Api.Entities;
using QTKD.Api.Services;
using Xunit;

namespace QTKD.Api.Tests.Unit;

[Trait("Category", "Unit")]
public class ContractServiceTests
{
    [Theory]
    [InlineData(Contract.DraftStatus)]
    [InlineData(Contract.RevisionRequestedStatus)]
    public async Task CTR01_UpdateEditableContract_UpdatesFieldsAndPreservesStatus(string status)
    {
        var contract = ValidContract();
        contract.Status = status;
        var repository = new FakeContractRepository(contract);
        var service = Service(repository, new Customer { Id = 8, Name = "Customer B", Status = Customer.ActiveStatus });
        var request = Request();
        request.CustomerId = 8;

        var result = await service.UpdateAsync(contract.Id, request, default);

        Assert.Equal(42, contract.Id);
        Assert.Equal(request.ContractNo.Trim(), contract.ContractNo);
        Assert.Equal(request.CustomerId, contract.CustomerId);
        Assert.Equal(request.StartDate, contract.StartDate);
        Assert.Equal(request.EndDate, contract.EndDate);
        Assert.Equal(request.Content!.Trim(), contract.Content);
        Assert.Equal(request.DocumentName!.Trim(), contract.DocumentName);
        Assert.Equal(status, contract.Status);
        AssertResponse(contract, result);
        Assert.Equal(1, repository.SaveCalls);
    }

    [Theory]
    [InlineData(Contract.PendingApprovalStatus)]
    [InlineData("Approved")]
    public async Task CTR01_UpdateLockedContract_RejectsWithoutMutation(string status)
    {
        var contract = ValidContract();
        contract.Status = status;
        var before = Snapshot(contract);
        var repository = new FakeContractRepository(contract);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Service(repository, contract.Customer).UpdateAsync(contract.Id, Request(), default));

        Assert.Equal("CTR-01: A contract can only be updated in Draft or RevisionRequested status.", error.Message);
        Assert.Equal(before, Snapshot(contract));
        Assert.Equal(0, repository.SaveCalls);
    }

    [Theory]
    [InlineData("inactive", "Customer must be Active.")]
    [InlineData("start-default", "The contract effective dates are invalid.")]
    [InlineData("end-default", "The contract effective dates are invalid.")]
    [InlineData("reversed", "The contract effective dates are invalid.")]
    [InlineData("content-null", "Contract content is required.")]
    [InlineData("content-empty", "Contract content is required.")]
    [InlineData("content-whitespace", "Contract content is required.")]
    [InlineData("document-null", "A required document is missing.")]
    [InlineData("document-empty", "A required document is missing.")]
    [InlineData("document-whitespace", "A required document is missing.")]
    public async Task CTR02_SubmitInvalidContract_ReportsExactErrorWithoutMutation(string scenario, string expected)
    {
        var contract = ValidContract();
        switch (scenario)
        {
            case "inactive": contract.Customer.Status = Customer.InactiveStatus; break;
            case "start-default": contract.StartDate = default; break;
            case "end-default": contract.EndDate = default; break;
            case "reversed": contract.EndDate = contract.StartDate.AddDays(-1); break;
            case "content-null": contract.Content = null!; break;
            case "content-empty": contract.Content = ""; break;
            case "content-whitespace": contract.Content = " \t "; break;
            case "document-null": contract.DocumentName = null!; break;
            case "document-empty": contract.DocumentName = ""; break;
            case "document-whitespace": contract.DocumentName = " \t "; break;
            default: throw new ArgumentOutOfRangeException(nameof(scenario));
        }
        var before = Snapshot(contract);
        var repository = new FakeContractRepository(contract);

        var error = await Assert.ThrowsAsync<ContractValidationException>(
            () => Service(repository, contract.Customer).SubmitAsync(contract.Id, default));

        Assert.Equal("The contract does not meet CTR-02.", error.Message);
        Assert.Equal(new[] { expected }, error.Errors);
        Assert.Equal(before, Snapshot(contract));
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task CTR03_CreateContract_AlwaysStartsInDraft()
    {
        var customer = ValidContract().Customer;
        var repository = new FakeContractRepository();
        var request = Request();

        var result = await Service(repository, customer).CreateAsync(request, default);

        var contract = Assert.IsType<Contract>(repository.Contract);
        Assert.Equal(42, contract.Id);
        Assert.Equal(request.ContractNo.Trim(), contract.ContractNo);
        Assert.Equal(request.CustomerId, contract.CustomerId);
        Assert.Equal(request.StartDate, contract.StartDate);
        Assert.Equal(request.EndDate, contract.EndDate);
        Assert.Equal(request.Content!.Trim(), contract.Content);
        Assert.Equal(request.DocumentName!.Trim(), contract.DocumentName);
        Assert.Equal(Contract.DraftStatus, contract.Status);
        AssertResponse(contract, result);
        Assert.Equal(1, repository.SaveCalls);
    }

    [Fact]
    public async Task CTR02_CTR03_SubmitValidDraft_OnlyChangesStatusToPendingApproval()
    {
        var contract = ValidContract();
        var repository = new FakeContractRepository(contract);
        var expected = Snapshot(contract) with { Status = Contract.PendingApprovalStatus };

        var result = await Service(repository, contract.Customer).SubmitAsync(contract.Id, default);

        Assert.Equal(expected, Snapshot(contract));
        Assert.Equal(contract.Id, result.ContractId);
        Assert.Equal(Contract.PendingApprovalStatus, result.Status);
        Assert.Equal("Contract submitted for approval.", result.Message);
        Assert.Equal(1, repository.SaveCalls);
    }

    [Theory]
    [InlineData(Contract.RevisionRequestedStatus)]
    [InlineData(Contract.PendingApprovalStatus)]
    [InlineData("Approved")]
    public async Task CTR03_SubmitNonDraft_RejectsWithoutMutation(string status)
    {
        var contract = ValidContract();
        contract.Status = status;
        var before = Snapshot(contract);
        var repository = new FakeContractRepository(contract);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Service(repository, contract.Customer).SubmitAsync(contract.Id, default));

        Assert.Equal("The contract must be in Draft status before submission.", error.Message);
        Assert.Equal(before, Snapshot(contract));
        Assert.Equal(0, repository.SaveCalls);
    }

    private static ContractService Service(FakeContractRepository repository, Customer customer)
        => new(repository, new FakeCustomerRepository(customer));

    private static ContractRequestDto Request() => new()
    {
        ContractNo = " NEW-02 ", CustomerId = 7,
        StartDate = new(2026, 12, 1), EndDate = new(2027, 1, 1),
        Content = " Updated content ", DocumentName = " updated.pdf "
    };

    private static Contract ValidContract() => new()
    {
        Id = 42, ContractNo = "OLD-01", CustomerId = 7,
        StartDate = new(2026, 11, 1), EndDate = new(2026, 11, 30),
        Content = "Original content", DocumentName = "original.pdf", Status = Contract.DraftStatus,
        Customer = new Customer { Id = 7, Name = "Customer A", Status = Customer.ActiveStatus }
    };

    private static void AssertResponse(Contract contract, ContractResponseDto result)
    {
        Assert.Equal(contract.Id, result.Id);
        Assert.Equal(contract.ContractNo, result.ContractNo);
        Assert.Equal(contract.CustomerId, result.CustomerId);
        Assert.Equal(contract.StartDate, result.StartDate);
        Assert.Equal(contract.EndDate, result.EndDate);
        Assert.Equal(contract.Status, result.Status);
        Assert.Equal(contract.Content, result.Content);
        Assert.Equal(contract.DocumentName, result.DocumentName);
    }

    // Capture every scalar field plus navigation identity and customer values.
    private static ContractSnapshot Snapshot(Contract contract) => new(
        contract.Id, contract.ContractNo, contract.CustomerId, contract.StartDate, contract.EndDate,
        contract.Status, contract.Content, contract.DocumentName, contract.Customer,
        contract.Customer.Id, contract.Customer.Name, contract.Customer.Status);

    private sealed record ContractSnapshot(
        int Id, string ContractNo, int CustomerId, DateOnly StartDate, DateOnly EndDate,
        string Status, string? Content, string? DocumentName, Customer Customer,
        int CustomerEntityId, string CustomerName, string CustomerStatus);
}
