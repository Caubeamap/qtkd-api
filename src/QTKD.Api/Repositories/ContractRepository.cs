using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly QtkdDbContext _dbContext;

    public ContractRepository(QtkdDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Contract?> GetByIdAsync(int id, bool trackChanges, CancellationToken cancellationToken)
    {
        var query = _dbContext.Contracts
            .Include(contract => contract.Customer)
            .AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query
            .FirstOrDefaultAsync(contract => contract.Id == id, cancellationToken);
    }

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken)
    {
        await _dbContext.Contracts.AddAsync(contract, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
