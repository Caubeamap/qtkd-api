using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public interface IPriceListRepository
{
    Task<List<PriceList>> GetAllAsync(CancellationToken cancellationToken);
    Task<PriceList?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<PriceItem>> GetItemsAsync(int priceListId, CancellationToken cancellationToken);
    Task<bool> ItemExistsAsync(int priceListId, string serviceCode, CancellationToken cancellationToken);
    Task AddAsync(PriceList priceList, CancellationToken cancellationToken);
    Task AddItemAsync(PriceItem priceItem, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
