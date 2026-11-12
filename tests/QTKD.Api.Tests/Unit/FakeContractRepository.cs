using QTKD.Api.Entities;
using QTKD.Api.Repositories;

namespace QTKD.Api.Tests.Unit;

internal sealed class FakeContractRepository(Contract? contract = null) : IContractRepository
{
    public Contract? Contract { get; private set; } = contract;
    public int SaveCalls { get; private set; }

    public Task<Contract?> GetByIdAsync(int id, bool trackChanges, CancellationToken cancellationToken)
        => Task.FromResult(Contract?.Id == id ? Contract : null);

    public Task AddAsync(Contract contract, CancellationToken cancellationToken)
    {
        contract.Id = 42;
        Contract = contract;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCalls++;
        return Task.CompletedTask;
    }
}
