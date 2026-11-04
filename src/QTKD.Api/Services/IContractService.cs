using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IContractService
{
    Task<ContractResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ContractResponseDto> CreateAsync(ContractRequestDto request, CancellationToken cancellationToken);
    Task<ContractResponseDto> UpdateAsync(int id, ContractRequestDto request, CancellationToken cancellationToken);
    Task<SubmitContractResponseDto> SubmitAsync(int id, CancellationToken cancellationToken);
}
