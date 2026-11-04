using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IPriceListService
{
    Task<List<PriceListResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<PriceListResponseDto> CreateAsync(PriceListRequestDto request, CancellationToken cancellationToken);
    Task<List<PriceItemResponseDto>> GetItemsAsync(int priceListId, CancellationToken cancellationToken);
    Task<PriceItemResponseDto> AddItemAsync(
        int priceListId,
        PriceItemRequestDto request,
        CancellationToken cancellationToken);
}
