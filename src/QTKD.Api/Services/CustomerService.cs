using QTKD.Api.DTOs;
using QTKD.Api.Entities;
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

        return customers.Select(Map).ToList();
    }

    public async Task<CustomerResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : Map(customer);
    }

    public async Task<CustomerResponseDto> CreateAsync(
        CustomerRequestDto request,
        CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Status = NormalizeStatus(request.Status)
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    public async Task<CustomerResponseDto?> UpdateAsync(
        int id,
        CustomerRequestDto request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        customer.Name = request.Name.Trim();
        customer.Status = NormalizeStatus(request.Status);

        await _customerRepository.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    private static string NormalizeStatus(string status)
    {
        if (status.Equals(Customer.ActiveStatus, StringComparison.OrdinalIgnoreCase))
        {
            return Customer.ActiveStatus;
        }

        if (status.Equals(Customer.InactiveStatus, StringComparison.OrdinalIgnoreCase))
        {
            return Customer.InactiveStatus;
        }

        throw new ArgumentException("Customer status must be Active or Inactive.");
    }

    private static CustomerResponseDto Map(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Status = customer.Status
        };
    }
}
