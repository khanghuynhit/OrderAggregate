using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderAggregate.Application;
using OrderAggregate.Domain;

namespace OrderAggregate.API;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateOrderCommand(request.CustomerId);
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetOrder),
                new { id = result.Id }, result);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        return NotFound();
    }

    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddItem(
        Guid id,
        [FromBody] AddItemRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AddOrderItemCommand(
                id,
                request.ProductId,
                request.ProductName,
                request.UnitPrice,
                request.Quantity);

            await _mediator.Send(command, cancellationToken);

            return Accepted();
        }
        catch (InvalidOrderStateException ex)
        {
            _logger.LogWarning("Invalid order state: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}/items/{productId}")]
    public async Task<IActionResult> RemoveItem(
        Guid id,
        Guid productId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RemoveOrderItemCommand(id, productId);
            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }
        catch (InvalidOrderStateException ex)
        {
            _logger.LogWarning("Invalid order state: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CompleteOrderCommand(id);
            await _mediator.Send(command, cancellationToken);

            return Accepted();
        }
        catch (InvalidOrderStateException ex)
        {
            _logger.LogWarning("Invalid order state: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CancelOrderCommand(id);
            await _mediator.Send(command, cancellationToken);

            return Accepted();
        }
        catch (InvalidOrderStateException ex)
        {
            _logger.LogWarning("Invalid order state: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record CreateOrderRequest(Guid CustomerId);

public record AddItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);