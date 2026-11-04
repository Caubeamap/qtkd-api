namespace QTKD.Api.DTOs;

public class PriceListResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
}
