using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
