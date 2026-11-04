using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/price-lists")]
public class PriceListsController : ControllerBase
{
    private readonly IPriceListService _priceListService;

    public PriceListsController(IPriceListService priceListService)
    {
        _priceListService = priceListService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PriceListResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PriceListResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _priceListService.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PriceListResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriceListResponseDto>> Create(
        PriceListRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var priceList = await _priceListService.CreateAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, priceList);
        }
        catch (ArgumentException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpGet("{id:int}/items")]
    [ProducesResponseType(typeof(List<PriceItemResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PriceItemResponseDto>>> GetItems(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _priceListService.GetItemsAsync(id, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status404NotFound);
        }
    }

    [HttpPost("{id:int}/items")]
    [ProducesResponseType(typeof(PriceItemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PriceItemResponseDto>> AddItem(
        int id,
        PriceItemRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _priceListService.AddItemAsync(id, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, item);
        }
        catch (KeyNotFoundException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status404NotFound);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }
}
