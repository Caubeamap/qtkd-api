using QTKD.Api.DTOs;
using QTKD.Api.Entities;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class PriceListService : IPriceListService
{
    private readonly IPriceListRepository _priceListRepository;

    public PriceListService(IPriceListRepository priceListRepository)
    {
        _priceListRepository = priceListRepository;
    }

    public async Task<List<PriceListResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var priceLists = await _priceListRepository.GetAllAsync(cancellationToken);
        return priceLists.Select(Map).ToList();
    }

    public async Task<PriceListResponseDto> CreateAsync(
        PriceListRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.EffectiveDate == default)
        {
            throw new ArgumentException("EffectiveDate is required.");
        }

        var priceList = new PriceList
        {
            Name = request.Name.Trim(),
            EffectiveDate = request.EffectiveDate
        };

        await _priceListRepository.AddAsync(priceList, cancellationToken);
        await _priceListRepository.SaveChangesAsync(cancellationToken);
        return Map(priceList);
    }

    public async Task<List<PriceItemResponseDto>> GetItemsAsync(
        int priceListId,
        CancellationToken cancellationToken)
    {
        await EnsurePriceListExistsAsync(priceListId, cancellationToken);
        var items = await _priceListRepository.GetItemsAsync(priceListId, cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<PriceItemResponseDto> AddItemAsync(
        int priceListId,
        PriceItemRequestDto request,
        CancellationToken cancellationToken)
    {
        await EnsurePriceListExistsAsync(priceListId, cancellationToken);

        var serviceCode = request.ServiceCode.Trim().ToUpperInvariant();
        if (await _priceListRepository.ItemExistsAsync(priceListId, serviceCode, cancellationToken))
        {
            throw new InvalidOperationException("The ServiceCode already exists in this PriceList.");
        }

        var item = new PriceItem
        {
            PriceListId = priceListId,
            ServiceCode = serviceCode,
            UnitPrice = request.UnitPrice
        };

        await _priceListRepository.AddItemAsync(item, cancellationToken);
        await _priceListRepository.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    private async Task EnsurePriceListExistsAsync(int id, CancellationToken cancellationToken)
    {
        if (await _priceListRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            throw new KeyNotFoundException("PriceList not found.");
        }
    }

    private static PriceListResponseDto Map(PriceList priceList)
    {
        return new PriceListResponseDto
        {
            Id = priceList.Id,
            Name = priceList.Name,
            EffectiveDate = priceList.EffectiveDate
        };
    }

    private static PriceItemResponseDto Map(PriceItem item)
    {
        return new PriceItemResponseDto
        {
            PriceListId = item.PriceListId,
            ServiceCode = item.ServiceCode,
            UnitPrice = item.UnitPrice
        };
    }
}
