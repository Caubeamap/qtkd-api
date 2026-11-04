using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface ICustomerService
{
    Task<List<CustomerResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<CustomerResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CustomerResponseDto> CreateAsync(CustomerRequestDto request, CancellationToken cancellationToken);
    Task<CustomerResponseDto?> UpdateAsync(int id, CustomerRequestDto request, CancellationToken cancellationToken);
}
