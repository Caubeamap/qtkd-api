using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IContractService
{
    Task<ContractResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
