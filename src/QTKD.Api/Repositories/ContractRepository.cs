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

    public Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _dbContext.Contracts
            .AsNoTracking()
            .FirstOrDefaultAsync(contract => contract.Id == id, cancellationToken);
    }
}
