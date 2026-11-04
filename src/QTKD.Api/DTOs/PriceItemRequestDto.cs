using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

public class PriceItemRequestDto
{
    [Required]
    public string ServiceCode { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal UnitPrice { get; set; }
}
