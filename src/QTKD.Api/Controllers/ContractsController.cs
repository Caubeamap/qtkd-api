using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/contracts")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        if (contract is null)
        {
            return NotFound();
        }

        return Ok(contract);
    }
}
