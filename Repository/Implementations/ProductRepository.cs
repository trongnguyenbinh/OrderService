using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Models;
using Model.RequestModels;
using Repository.Interfaces;

namespace Repository.Implementations;

public class ProductRepository : IProductRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(DataContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product by ID: {ProductId}", id);
        
        try
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            
            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", id);
            }
            
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ProductEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all products");
        
        try
        {
            var products = await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} products", products.Count);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            throw;
        }
    }

    public async Task<ProductEntity> AddAsync(ProductEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new product with SKU: {SKU}", entity.SKU);
        
        try
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await _context.Products.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully added product with ID: {ProductId}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product with SKU: {SKU}", entity.SKU);
            throw;
        }
    }

    public async Task<ProductEntity> UpdateAsync(ProductEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", entity.Id);
        
        try
        {
            var existingProduct = await _context.Products.FindAsync(new object[] { entity.Id }, cancellationToken);
            
            if (existingProduct == null)
            {
                _logger.LogWarning("Product not found for update with ID: {ProductId}", entity.Id);
                throw new KeyNotFoundException($"Product with ID {entity.Id} not found");
            }
            
            existingProduct.Name = entity.Name;
            existingProduct.Description = entity.Description;
            existingProduct.SKU = entity.SKU;
            existingProduct.Price = entity.Price;
            existingProduct.StockQuantity = entity.StockQuantity;
            existingProduct.Category = entity.Category;
            existingProduct.IsActive = entity.IsActive;
            existingProduct.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully updated product with ID: {ProductId}", entity.Id);
            return existingProduct;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID: {ProductId}", entity.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);
        
        try
        {
            var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
            
            if (product == null)
            {
                _logger.LogWarning("Product not found for deletion with ID: {ProductId}", id);
                return false;
            }
            
            // Soft delete by setting IsActive to false
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Successfully soft deleted product with ID: {ProductId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<ProductEntity?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product by SKU: {SKU}", sku);

        try
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);

            if (product == null)
            {
                _logger.LogWarning("Product not found with SKU: {SKU}", sku);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by SKU: {SKU}", sku);
            throw;
        }
    }

    public async Task<PagedResult<ProductEntity>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Searching products - Name: {Name}, Description: {Description}, SKU: {SKU}, Category: {Category}, Page: {PageNumber}, PageSize: {PageSize}, SortBy: {SortBy}, SortDirection: {SortDirection}",
            request.Name, request.Description, request.SKU, request.Category, request.PageNumber, request.PageSize, request.SortBy, request.SortDirection);

        try
        {
            // Start with base query - filter by IsActive = true
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive == true);

            // Apply search filters
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var nameLower = request.Name.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(nameLower));
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                var descriptionLower = request.Description.ToLower();
                query = query.Where(p => p.Description != null && p.Description.ToLower().Contains(descriptionLower));
            }

            if (!string.IsNullOrWhiteSpace(request.SKU))
            {
                var skuLower = request.SKU.ToLower();
                query = query.Where(p => p.SKU.ToLower().Contains(skuLower));
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                query = query.Where(p => p.Category == request.Category);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDirection);

            // Apply pagination
            var skip = (request.PageNumber - 1) * request.PageSize;
            string sql = query.ToQueryString();
            Console.WriteLine(sql);
            var items = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            _logger.LogInformation("Found {TotalCount} products matching search criteria, returning page {PageNumber} of {TotalPages}",
                totalCount, request.PageNumber, totalPages);

            return new PagedResult<ProductEntity>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            throw;
        }
    }

    private static IQueryable<ProductEntity> ApplySorting(IQueryable<ProductEntity> query, string? sortBy, string sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "price" => isDescending
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            "stockquantity" => isDescending
                ? query.OrderByDescending(p => p.StockQuantity)
                : query.OrderBy(p => p.StockQuantity),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
    }
}

