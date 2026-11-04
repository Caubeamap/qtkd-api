namespace QTKD.Api.DTOs;

public class SubmitContractResponseDto
{
    public int ContractId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
