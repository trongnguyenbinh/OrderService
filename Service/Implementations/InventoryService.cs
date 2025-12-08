using Common.Exceptions;
using Microsoft.Extensions.Logging;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class InventoryService : IInventoryService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IProductRepository productRepository, ILogger<InventoryService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task ValidateStockAvailabilityAsync(Guid productId, int requestedQuantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating stock availability for product: {ProductId}, Quantity: {Quantity}", 
            productId, requestedQuantity);

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", productId);
            throw new NotFoundException("Product", productId);
        }

        if (product.StockQuantity < requestedQuantity)
        {
            _logger.LogWarning("Insufficient stock for product: {ProductName}. Available: {Available}, Requested: {Requested}", 
                product.Name, product.StockQuantity, requestedQuantity);
            throw new InsufficientStockException(product.Name, product.StockQuantity, requestedQuantity);
        }

        _logger.LogInformation("Stock validation passed for product: {ProductName}", product.Name);
    }

    public async Task<bool> CheckStockAvailabilityAsync(Guid productId, int requestedQuantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking stock availability for product: {ProductId}, Quantity: {Quantity}", 
            productId, requestedQuantity);

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", productId);
            return false;
        }

        var isAvailable = product.StockQuantity >= requestedQuantity;
        
        _logger.LogInformation("Stock check result for product {ProductName}: {IsAvailable}", 
            product.Name, isAvailable);

        return isAvailable;
    }

    public async Task ReduceStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reducing stock for product: {ProductId}, Quantity: {Quantity}",
            productId, quantity);

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", productId);
            throw new NotFoundException("Product", productId);
        }

        if (product.StockQuantity < quantity)
        {
            _logger.LogWarning("Insufficient stock for product: {ProductName}. Available: {Available}, Requested: {Requested}",
                product.Name, product.StockQuantity, quantity);
            throw new InsufficientStockException(product.Name, product.StockQuantity, quantity);
        }

        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product, cancellationToken);

        _logger.LogInformation("Successfully reduced stock for product: {ProductName}. New stock: {NewStock}",
            product.Name, product.StockQuantity);
    }

    public async Task ValidateBulkStockAvailabilityAsync(Dictionary<Guid, int> productQuantities, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating stock availability for {Count} products in bulk", productQuantities.Count);

        // Fetch all products in a single query
        var products = await _productRepository.GetByIdsAsync(productQuantities.Keys, cancellationToken);

        // Validate all products exist
        foreach (var productId in productQuantities.Keys)
        {
            if (!products.ContainsKey(productId))
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                throw new NotFoundException("Product", productId);
            }
        }

        // Validate stock availability for all products
        foreach (var (productId, requestedQuantity) in productQuantities)
        {
            var product = products[productId];

            if (product.StockQuantity < requestedQuantity)
            {
                _logger.LogWarning("Insufficient stock for product: {ProductName}. Available: {Available}, Requested: {Requested}",
                    product.Name, product.StockQuantity, requestedQuantity);
                throw new InsufficientStockException(product.Name, product.StockQuantity, requestedQuantity);
            }
        }

        _logger.LogInformation("Stock validation passed for all {Count} products", productQuantities.Count);
    }

    public async Task ReduceBulkStockAsync(Dictionary<Guid, int> productQuantities, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reducing stock for {Count} products in bulk", productQuantities.Count);

        // Fetch all products in a single query
        var products = await _productRepository.GetByIdsAsync(productQuantities.Keys, cancellationToken);

        // Validate all products exist
        foreach (var productId in productQuantities.Keys)
        {
            if (!products.ContainsKey(productId))
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                throw new NotFoundException("Product", productId);
            }
        }

        // Reduce stock for all products
        var productsToUpdate = new List<Domain.Entities.ProductEntity>();

        foreach (var (productId, quantity) in productQuantities)
        {
            var product = products[productId];

            if (product.StockQuantity < quantity)
            {
                _logger.LogWarning("Insufficient stock for product: {ProductName}. Available: {Available}, Requested: {Requested}",
                    product.Name, product.StockQuantity, quantity);
                throw new InsufficientStockException(product.Name, product.StockQuantity, quantity);
            }

            product.StockQuantity -= quantity;
            product.UpdatedAt = DateTime.UtcNow;
            productsToUpdate.Add(product);

            _logger.LogInformation("Reduced stock for product: {ProductName}. New stock: {NewStock}",
                product.Name, product.StockQuantity);
        }

        // Update all products in a single database operation
        await _productRepository.UpdateRangeAsync(productsToUpdate, cancellationToken);

        _logger.LogInformation("Successfully reduced stock for all {Count} products in bulk", productQuantities.Count);
    }

    public async Task ReturnBulkStockAsync(Dictionary<Guid, int> productQuantities, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Returning stock for {Count} products in bulk", productQuantities.Count);

        // Fetch all products in a single query
        var products = await _productRepository.GetByIdsAsync(productQuantities.Keys, cancellationToken);

        // Validate all products exist
        foreach (var productId in productQuantities.Keys)
        {
            if (!products.ContainsKey(productId))
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                throw new NotFoundException("Product", productId);
            }
        }

        // Return stock for all products
        var productsToUpdate = new List<Domain.Entities.ProductEntity>();

        foreach (var (productId, quantity) in productQuantities)
        {
            var product = products[productId];

            product.StockQuantity += quantity;
            product.UpdatedAt = DateTime.UtcNow;
            productsToUpdate.Add(product);

            _logger.LogInformation("Returned stock for product: {ProductName}. New stock: {NewStock}",
                product.Name, product.StockQuantity);
        }

        // Update all products in a single database operation
        await _productRepository.UpdateRangeAsync(productsToUpdate, cancellationToken);

        _logger.LogInformation("Successfully returned stock for all {Count} products in bulk", productQuantities.Count);
    }
}

