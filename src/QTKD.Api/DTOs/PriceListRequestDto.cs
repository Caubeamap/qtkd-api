using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

public class PriceListRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public DateOnly EffectiveDate { get; set; }
}
