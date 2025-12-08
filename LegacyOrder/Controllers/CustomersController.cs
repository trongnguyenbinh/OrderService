using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Model.RequestModels;
using Service.Interfaces;

namespace LegacyOrder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Search and filter customers with pagination
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchCustomers([FromQuery] CustomerSearchRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "API: Searching customers - FirstName: {FirstName}, LastName: {LastName}, Email: {Email}, PhoneNumber: {PhoneNumber}, CustomerType: {CustomerType}, Page: {PageNumber}, PageSize: {PageSize}, SortBy: {SortBy}, SortDirection: {SortDirection}",
            request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.CustomerType, request.PageNumber, request.PageSize, request.SortBy, request.SortDirection);

        var result = await _customerService.SearchAsync(request, cancellationToken);

        _logger.LogInformation("API: Successfully retrieved {Count} customers (Page {PageNumber} of {TotalPages})",
            result.Items.Count(), result.PageNumber, result.TotalPages);
        return Ok(result);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Getting customer by ID: {CustomerId}", id);

        var customer = await _customerService.GetByIdAsync(id, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("API: Customer not found with ID: {CustomerId}", id);
            return NotFound(new { error = $"Customer with ID {id} not found" });
        }

        _logger.LogInformation("API: Successfully retrieved customer with ID: {CustomerId}", id);
        return Ok(customer);
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Creating new customer with Email: {Email}", request.Email);

        var customer = await _customerService.CreateAsync(request, cancellationToken);

        _logger.LogInformation("API: Successfully created customer with ID: {CustomerId}", customer.Id);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Updating customer with ID: {CustomerId}", id);

        var customer = await _customerService.UpdateAsync(id, request, cancellationToken);

        _logger.LogInformation("API: Successfully updated customer with ID: {CustomerId}", id);
        return Ok(customer);
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Deleting customer with ID: {CustomerId}", id);

        var result = await _customerService.DeleteAsync(id, cancellationToken);

        if (!result)
        {
            _logger.LogWarning("API: Customer not found for deletion with ID: {CustomerId}", id);
            return NotFound(new { error = $"Customer with ID {id} not found" });
        }

        _logger.LogInformation("API: Successfully deleted customer with ID: {CustomerId}", id);
        return NoContent();
    }

}

