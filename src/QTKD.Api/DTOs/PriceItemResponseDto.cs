namespace QTKD.Api.DTOs;

public class PriceItemResponseDto
{
    public int PriceListId { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
