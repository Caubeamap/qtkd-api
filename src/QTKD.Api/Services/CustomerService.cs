using QTKD.Api.DTOs;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<CustomerResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);

        return customers.Select(customer => new CustomerResponseDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Status = customer.Status
        }).ToList();
    }
}
