using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CustomerResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(cancellationToken);
        return Ok(customers);
    }
}
