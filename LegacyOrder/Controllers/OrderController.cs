using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Models;
using Model.RequestModels;
using Service.Interfaces;
using Common.Exceptions;

namespace LegacyOrder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders with pagination and optional status filter
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="orderStatus">Optional order status filter (Pending, Completed, Cancelled)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] OrderStatus? orderStatus = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("API: Getting all orders - Page: {PageNumber}, PageSize: {PageSize}, Status: {OrderStatus}",
            pageNumber, pageSize, orderStatus?.ToString() ?? "All");

        if (pageNumber < 1)
        {
            return BadRequest(new { error = "Page number must be greater than or equal to 1" });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "Page size must be between 1 and 100" });
        }

        var result = await _orderService.GetAllAsync(pageNumber, pageSize, orderStatus, cancellationToken);

        _logger.LogInformation("API: Successfully retrieved {Count} orders (Page {PageNumber} of {TotalPages}) with status filter: {OrderStatus}",
            result.Items.Count(), result.PageNumber, result.TotalPages, orderStatus?.ToString() ?? "All");
        return Ok(result);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Getting order by ID: {OrderId}", id);

        var order = await _orderService.GetByIdAsync(id, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("API: Order not found with ID: {OrderId}", id);
            return NotFound(new { error = $"Order with ID {id} not found" });
        }

        _logger.LogInformation("API: Successfully retrieved order with ID: {OrderId}", id);
        return Ok(order);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Creating new order for customer: {CustomerId}", request.CustomerId);

        try
        {
            var order = await _orderService.CreateOrderAsync(request, cancellationToken);

            _logger.LogInformation("API: Successfully created order with ID: {OrderId}, OrderNumber: {OrderNumber}",
                order.Id, order.OrderNumber);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("API: Order creation failed - {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (InsufficientStockException ex)
        {
            _logger.LogWarning("API: Order creation failed - Insufficient stock: {Message}", ex.Message);
            return BadRequest(new
            {
                error = ex.Message,
                errorCode = "INSUFFICIENT_STOCK",
                productName = ex.ProductName,
                availableStock = ex.AvailableStock,
                requestedQuantity = ex.RequestedQuantity
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("API: Order creation failed - Validation error: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Mark order as completed
    /// </summary>
    [HttpPut("{id}/complete")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Completing order with ID: {OrderId}", id);

        try
        {
            var order = await _orderService.CompleteOrderAsync(id, cancellationToken);

            _logger.LogInformation("API: Successfully completed order with ID: {OrderId}", id);
            return Ok(order);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("API: Order completion failed - {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOrderStatusException ex)
        {
            _logger.LogWarning("API: Order completion failed - Invalid status: {Message}", ex.Message);
            return BadRequest(new
            {
                error = ex.Message,
                errorCode = "INVALID_ORDER_STATUS"
            });
        }
    }

    /// <summary>
    /// Cancel order (only if Pending)
    /// </summary>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Cancelling order with ID: {OrderId}", id);

        try
        {
            var order = await _orderService.CancelOrderAsync(id, cancellationToken);

            _logger.LogInformation("API: Successfully cancelled order with ID: {OrderId}", id);
            return Ok(order);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("API: Order cancellation failed - {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOrderStatusException ex)
        {
            _logger.LogWarning("API: Order cancellation failed - Invalid status: {Message}", ex.Message);
            return BadRequest(new
            {
                error = ex.Message,
                errorCode = "INVALID_ORDER_STATUS"
            });
        }
    }
}
