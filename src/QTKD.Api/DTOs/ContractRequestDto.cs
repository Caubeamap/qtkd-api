using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QTKD.Api.DTOs;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class ContractRequestDto
{
    [Required]
    public string ContractNo { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Content { get; set; }
    public string? DocumentName { get; set; }
}
