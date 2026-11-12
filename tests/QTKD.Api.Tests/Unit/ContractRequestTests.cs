using System.Text.Json;
using QTKD.Api.DTOs;
using Xunit;

namespace QTKD.Api.Tests.Unit;

[Trait("Category", "Unit")]
public class ContractRequestTests
{
    [Fact]
    public void CTR03_RequestWithApprovedStatus_IsRejected()
    {
        const string json = """
            {"contractNo":"HD-01","customerId":1,"startDate":"2026-11-01",
             "endDate":"2026-12-01","content":"Content","documentName":"contract.pdf",
             "status":"Approved"}
            """;

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ContractRequestDto>(
            json, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
