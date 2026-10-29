using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken);
}
