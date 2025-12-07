using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Model.RequestModels;
using Service.Interfaces;

namespace LegacyOrder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Search and filter products with pagination
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "API: Searching products - Name: {Name}, Description: {Description}, SKU: {SKU}, Category: {Category}, Page: {PageNumber}, PageSize: {PageSize}, SortBy: {SortBy}, SortDirection: {SortDirection}",
            request.Name, request.Description, request.SKU, request.Category, request.PageNumber, request.PageSize, request.SortBy, request.SortDirection);

        try
        {
            var result = await _productService.SearchAsync(request, cancellationToken);

            _logger.LogInformation("API: Successfully retrieved {Count} products (Page {PageNumber} of {TotalPages})",
                result.Items.Count(), result.PageNumber, result.TotalPages);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "API: Validation error searching products");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error searching products");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while searching products" });
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Getting product by ID: {ProductId}", id);
        
        try
        {
            var product = await _productService.GetByIdAsync(id, cancellationToken);
            
            if (product == null)
            {
                _logger.LogWarning("API: Product not found with ID: {ProductId}", id);
                return NotFound(new { error = $"Product with ID {id} not found" });
            }
            
            _logger.LogInformation("API: Successfully retrieved product with ID: {ProductId}", id);
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error getting product by ID: {ProductId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while retrieving the product" });
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Creating new product with SKU: {SKU}", request.SKU);
        
        try
        {
            var product = await _productService.CreateAsync(request, cancellationToken);
            
            _logger.LogInformation("API: Successfully created product with ID: {ProductId}", product.Id);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "API: Validation error creating product");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "API: Business rule violation creating product");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error creating product");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while creating the product" });
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Updating product with ID: {ProductId}", id);
        
        try
        {
            var product = await _productService.UpdateAsync(id, request, cancellationToken);
            
            _logger.LogInformation("API: Successfully updated product with ID: {ProductId}", id);
            return Ok(product);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "API: Product not found for update with ID: {ProductId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "API: Validation error updating product");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "API: Business rule violation updating product");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error updating product with ID: {ProductId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while updating the product" });
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Deleting product with ID: {ProductId}", id);

        try
        {
            var result = await _productService.DeleteAsync(id, cancellationToken);

            if (!result)
            {
                _logger.LogWarning("API: Product not found for deletion with ID: {ProductId}", id);
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            _logger.LogInformation("API: Successfully deleted product with ID: {ProductId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error deleting product with ID: {ProductId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while deleting the product" });
        }
    }

}

