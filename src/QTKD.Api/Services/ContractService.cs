using QTKD.Api.DTOs;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;

    public ContractService(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<ContractResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(id, cancellationToken);
        if (contract is null)
        {
            return null;
        }

        return new ContractResponseDto
        {
            Id = contract.Id,
            ContractNo = contract.ContractNo,
            CustomerId = contract.CustomerId,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status
        };
    }
}
