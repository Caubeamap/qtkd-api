using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly QtkdDbContext _dbContext;

    public CustomerRepository(QtkdDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.Id)
            .ToListAsync(cancellationToken);
    }
}
