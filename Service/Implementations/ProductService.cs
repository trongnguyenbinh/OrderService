using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Model.Models;
using Model.RequestModels;
using Service.Interfaces;

namespace Service.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger, IMapper mapper)
    {
        _productRepository = productRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Getting product by ID: {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Service: Product not found with ID: {ProductId}", id);
            return null;
        }

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Getting all products");

        var products = await _productRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Creating new product with SKU: {SKU}", request.SKU);
        
        // Validation
        ValidateCreateRequest(request);
        
        // Check if SKU already exists
        var existingProduct = await _productRepository.GetBySkuAsync(request.SKU, cancellationToken);
        if (existingProduct != null)
        {
            _logger.LogWarning("Service: Product with SKU {SKU} already exists", request.SKU);
            throw new InvalidOperationException($"Product with SKU '{request.SKU}' already exists");
        }
        
        var entity = new ProductEntity
        {
            Name = request.Name,
            Description = request.Description,
            SKU = request.SKU,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Category = request.Category,
            IsActive = request.IsActive
        };
        
        var createdProduct = await _productRepository.AddAsync(entity, cancellationToken);

        _logger.LogInformation("Service: Successfully created product with ID: {ProductId}", createdProduct.Id);

        return _mapper.Map<ProductDto>(createdProduct);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Updating product with ID: {ProductId}", id);
        
        // Validation
        ValidateUpdateRequest(request);
        
        // Check if product exists
        var existingProduct = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (existingProduct == null)
        {
            _logger.LogWarning("Service: Product not found for update with ID: {ProductId}", id);
            throw new KeyNotFoundException($"Product with ID {id} not found");
        }
        
        // Check if SKU is being changed and if new SKU already exists
        if (existingProduct.SKU != request.SKU)
        {
            var productWithSameSku = await _productRepository.GetBySkuAsync(request.SKU, cancellationToken);
            if (productWithSameSku != null && productWithSameSku.Id != id)
            {
                _logger.LogWarning("Service: SKU {SKU} is already used by another product", request.SKU);
                throw new InvalidOperationException($"SKU '{request.SKU}' is already used by another product");
            }
        }
        
        var entity = new ProductEntity
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            SKU = request.SKU,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Category = request.Category,
            IsActive = request.IsActive,
            CreatedAt = existingProduct.CreatedAt
        };
        
        var updatedProduct = await _productRepository.UpdateAsync(entity, cancellationToken);

        _logger.LogInformation("Service: Successfully updated product with ID: {ProductId}", id);

        return _mapper.Map<ProductDto>(updatedProduct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Deleting product with ID: {ProductId}", id);
        
        var result = await _productRepository.DeleteAsync(id, cancellationToken);
        
        if (result)
        {
            _logger.LogInformation("Service: Successfully deleted product with ID: {ProductId}", id);
        }
        else
        {
            _logger.LogWarning("Service: Product not found for deletion with ID: {ProductId}", id);
        }
        
        return result;
    }

    public async Task<ProductDto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Getting product by SKU: {SKU}", sku);

        var product = await _productRepository.GetBySkuAsync(sku, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Service: Product not found with SKU: {SKU}", sku);
            return null;
        }

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<PagedResult<ProductDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Service: Searching products - Name: {Name}, Description: {Description}, SKU: {SKU}, Category: {Category}, Page: {PageNumber}, PageSize: {PageSize}",
            request.Name, request.Description, request.SKU, request.Category, request.PageNumber, request.PageSize);

        // Validate pagination parameters
        ValidateSearchRequest(request);
        var pagedResult = await _productRepository.SearchAsync(request, cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<IEnumerable<ProductDto>>(pagedResult.Items),
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages
        };
    }

    private void ValidateCreateRequest(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Service: Validation failed - Product name is required");
            throw new ArgumentException("Product name is required", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.SKU))
        {
            _logger.LogWarning("Service: Validation failed - Product SKU is required");
            throw new ArgumentException("Product SKU is required", nameof(request));
        }

        if (request.Price <= 0)
        {
            _logger.LogWarning("Service: Validation failed - Product price must be greater than zero");
            throw new ArgumentException("Product price must be greater than zero", nameof(request));
        }

        if (request.StockQuantity < 0)
        {
            _logger.LogWarning("Service: Validation failed - Stock quantity cannot be negative");
            throw new ArgumentException("Stock quantity cannot be negative", nameof(request));
        }
    }

    private void ValidateUpdateRequest(UpdateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Service: Validation failed - Product name is required");
            throw new ArgumentException("Product name is required", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.SKU))
        {
            _logger.LogWarning("Service: Validation failed - Product SKU is required");
            throw new ArgumentException("Product SKU is required", nameof(request));
        }

        if (request.Price <= 0)
        {
            _logger.LogWarning("Service: Validation failed - Product price must be greater than zero");
            throw new ArgumentException("Product price must be greater than zero", nameof(request));
        }

        if (request.StockQuantity < 0)
        {
            _logger.LogWarning("Service: Validation failed - Stock quantity cannot be negative");
            throw new ArgumentException("Stock quantity cannot be negative", nameof(request));
        }
    }

    private void ValidateSearchRequest(ProductSearchRequest request)
    {
        const int maxPageSize = 100;

        if (request.PageNumber < 1)
        {
            _logger.LogWarning("Service: Validation failed - Page number must be greater than or equal to 1");
            throw new ArgumentException("Page number must be greater than or equal to 1", nameof(request));
        }

        if (request.PageSize < 1)
        {
            _logger.LogWarning("Service: Validation failed - Page size must be greater than 0");
            throw new ArgumentException("Page size must be greater than 0", nameof(request));
        }

        if (request.PageSize > maxPageSize)
        {
            _logger.LogWarning("Service: Validation failed - Page size cannot exceed {MaxPageSize}", maxPageSize);
            throw new ArgumentException($"Page size cannot exceed {maxPageSize}", nameof(request));
        }

        // Validate sort direction
        if (!string.IsNullOrWhiteSpace(request.SortDirection) &&
            request.SortDirection.ToLower() != "asc" &&
            request.SortDirection.ToLower() != "desc")
        {
            _logger.LogWarning("Service: Validation failed - Sort direction must be 'asc' or 'desc'");
            throw new ArgumentException("Sort direction must be 'asc' or 'desc'", nameof(request));
        }

        // Validate sort field
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var validSortFields = new[] { "price", "stockquantity" };
            if (!validSortFields.Contains(request.SortBy.ToLower()))
            {
                _logger.LogWarning("Service: Validation failed - Invalid sort field: {SortBy}", request.SortBy);
                throw new ArgumentException("Sort by must be either 'Price' or 'StockQuantity'", nameof(request));
            }
        }
    }
}

