using QTKD.Api.Entities;
using QTKD.Api.Repositories;

namespace QTKD.Api.Tests.Unit;

internal sealed class FakeCustomerRepository(Customer customer) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => Task.FromResult<Customer?>(customer.Id == id ? customer : null);

    public Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken)
        => Task.FromResult(new List<Customer> { customer });

    public Task AddAsync(Customer value, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => throw new NotSupportedException();
}
