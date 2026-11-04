using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public class PriceListRepository : IPriceListRepository
{
    private readonly QtkdDbContext _dbContext;

    public PriceListRepository(QtkdDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<PriceList>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _dbContext.PriceLists
            .AsNoTracking()
            .OrderBy(priceList => priceList.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<PriceList?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _dbContext.PriceLists
            .AsNoTracking()
            .FirstOrDefaultAsync(priceList => priceList.Id == id, cancellationToken);
    }

    public Task<List<PriceItem>> GetItemsAsync(int priceListId, CancellationToken cancellationToken)
    {
        return _dbContext.PriceItems
            .AsNoTracking()
            .Where(item => item.PriceListId == priceListId)
            .OrderBy(item => item.ServiceCode)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ItemExistsAsync(int priceListId, string serviceCode, CancellationToken cancellationToken)
    {
        return _dbContext.PriceItems.AnyAsync(
            item => item.PriceListId == priceListId && item.ServiceCode == serviceCode,
            cancellationToken);
    }

    public async Task AddAsync(PriceList priceList, CancellationToken cancellationToken)
    {
        await _dbContext.PriceLists.AddAsync(priceList, cancellationToken);
    }

    public async Task AddItemAsync(PriceItem priceItem, CancellationToken cancellationToken)
    {
        await _dbContext.PriceItems.AddAsync(priceItem, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
