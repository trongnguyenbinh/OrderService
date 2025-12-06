using Microsoft.AspNetCore.Mvc;
using LegacyOrderService.Models;
using Model.RequestModels;

namespace LegacyOrder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetOrderById(int id)
    {
        var order = new Order
        {
            CustomerName = "Sample Customer",
            ProductName = "Sample Product",
            Quantity = 5,
            Price = 29.99
        };

        return Ok(new
        {
            id = id,
            order = order,
            status = "Pending",
            createdAt = DateTime.UtcNow
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.CustomerName) ||
            string.IsNullOrWhiteSpace(request.ProductName) ||
            request.Quantity <= 0 ||
            request.Price <= 0)
        {
            return BadRequest(new { error = "Invalid order data. All fields are required and must be positive." });
        }

        var order = new Order
        {
            CustomerName = request.CustomerName,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            Price = request.Price
        };

        var orderId = new Random().Next(1000, 9999);

        return Created($"/api/orders/{orderId}", new
        {
            id = orderId,
            order = order,
            status = "Created",
            createdAt = DateTime.UtcNow
        });
    }
}
