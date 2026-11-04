using QTKD.Api.DTOs;
using QTKD.Api.Entities;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;
    private readonly ICustomerRepository _customerRepository;

    public ContractService(
        IContractRepository contractRepository,
        ICustomerRepository customerRepository)
    {
        _contractRepository = contractRepository;
        _customerRepository = customerRepository;
    }

    public async Task<ContractResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(id, false, cancellationToken);
        return contract is null ? null : Map(contract);
    }

    public async Task<ContractResponseDto> CreateAsync(
        ContractRequestDto request,
        CancellationToken cancellationToken)
    {
        await ValidateCustomerAsync(request.CustomerId, cancellationToken);
        ValidateDates(request.StartDate, request.EndDate);

        var contract = new Contract
        {
            ContractNo = request.ContractNo.Trim(),
            CustomerId = request.CustomerId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = Contract.DraftStatus,
            Content = request.Content?.Trim() ?? string.Empty,
            DocumentName = request.DocumentName?.Trim() ?? string.Empty
        };

        await _contractRepository.AddAsync(contract, cancellationToken);
        await _contractRepository.SaveChangesAsync(cancellationToken);
        return Map(contract);
    }

    public async Task<ContractResponseDto> UpdateAsync(
        int id,
        ContractRequestDto request,
        CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new KeyNotFoundException("Contract not found.");

        if (contract.Status is not Contract.DraftStatus and not Contract.RevisionRequestedStatus)
        {
            throw new InvalidOperationException(
                "CTR-01: A contract can only be updated in Draft or RevisionRequested status.");
        }

        await ValidateCustomerAsync(request.CustomerId, cancellationToken);
        ValidateDates(request.StartDate, request.EndDate);

        contract.ContractNo = request.ContractNo.Trim();
        contract.CustomerId = request.CustomerId;
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.Content = request.Content?.Trim() ?? string.Empty;
        contract.DocumentName = request.DocumentName?.Trim() ?? string.Empty;

        await _contractRepository.SaveChangesAsync(cancellationToken);
        return Map(contract);
    }

    public async Task<SubmitContractResponseDto> SubmitAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new KeyNotFoundException("Contract not found.");

        if (contract.Status != Contract.DraftStatus)
        {
            throw new InvalidOperationException(
                "The contract must be in Draft status before submission.");
        }

        var errors = new List<string>();
        if (contract.Customer.Status != Customer.ActiveStatus)
        {
            errors.Add("Customer must be Active.");
        }

        if (contract.StartDate == default || contract.EndDate == default || contract.StartDate > contract.EndDate)
        {
            errors.Add("The contract effective dates are invalid.");
        }

        if (string.IsNullOrWhiteSpace(contract.Content))
        {
            errors.Add("Contract content is required.");
        }

        if (string.IsNullOrWhiteSpace(contract.DocumentName))
        {
            errors.Add("A required document is missing.");
        }

        if (errors.Count > 0)
        {
            throw new ContractValidationException(errors);
        }

        contract.Status = Contract.PendingApprovalStatus;
        await _contractRepository.SaveChangesAsync(cancellationToken);

        return new SubmitContractResponseDto
        {
            ContractId = contract.Id,
            Status = contract.Status,
            Message = "Contract submitted for approval."
        };
    }

    private async Task ValidateCustomerAsync(int customerId, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null || customer.Status != Customer.ActiveStatus)
        {
            throw new ArgumentException("The referenced Customer must exist and be Active.");
        }
    }

    private static void ValidateDates(DateOnly startDate, DateOnly endDate)
    {
        if (startDate == default || endDate == default || startDate > endDate)
        {
            throw new ArgumentException("StartDate and EndDate are required, and StartDate must not be after EndDate.");
        }
    }

    private static ContractResponseDto Map(Contract contract)
    {
        return new ContractResponseDto
        {
            Id = contract.Id,
            ContractNo = contract.ContractNo,
            CustomerId = contract.CustomerId,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status,
            Content = contract.Content,
            DocumentName = contract.DocumentName
        };
    }
}
