using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(int id, bool trackChanges, CancellationToken cancellationToken);
    Task AddAsync(Contract contract, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
