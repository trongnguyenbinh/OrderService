namespace LegacyOrder.Tests.TestFixtures;

public class TestDataBuilder
{
    public static ProductEntity CreateProductEntity(
        Guid? id = null,
        string name = "Test Product",
        string? description = "Test Description",
        string sku = "TEST-SKU-001",
        decimal price = 99.99m,
        int stockQuantity = 10,
        string? category = "Electronics",
        bool isActive = true,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new ProductEntity
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            SKU = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category,
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow
        };
    }

    public static CreateProductRequest CreateProductRequest(
        string name = "Test Product",
        string? description = "Test Description",
        string sku = "TEST-SKU-001",
        decimal price = 99.99m,
        int stockQuantity = 10,
        string? category = "Electronics",
        bool isActive = true)
    {
        return new CreateProductRequest
        {
            Name = name,
            Description = description,
            SKU = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category,
            IsActive = isActive
        };
    }

    public static UpdateProductRequest CreateUpdateProductRequest(
        string name = "Updated Product",
        string? description = "Updated Description",
        string sku = "UPDATED-SKU-001",
        decimal price = 149.99m,
        int stockQuantity = 20,
        string? category = "Electronics",
        bool isActive = true)
    {
        return new UpdateProductRequest
        {
            Name = name,
            Description = description,
            SKU = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category,
            IsActive = isActive
        };
    }

    public static ProductSearchRequest CreateSearchRequest(
        string? name = null,
        string? description = null,
        string? sku = null,
        string? category = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        string sortDirection = "asc")
    {
        return new ProductSearchRequest
        {
            Name = name,
            Description = description,
            SKU = sku,
            Category = category,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
    }

    public static ProductDto CreateProductDto(
        Guid? id = null,
        string name = "Test Product",
        string? description = "Test Description",
        string sku = "TEST-SKU-001",
        decimal price = 99.99m,
        int stockQuantity = 10,
        string? category = "Electronics",
        bool isActive = true,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new ProductDto
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            SKU = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category,
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow
        };
    }

    public static List<ProductEntity> CreateProductEntityList(int count = 5)
    {
        var products = new List<ProductEntity>();
        for (int i = 1; i <= count; i++)
        {
            products.Add(CreateProductEntity(
                name: $"Product {i}",
                sku: $"SKU-{i:D3}",
                price: 10m * i,
                stockQuantity: i * 5
            ));
        }
        return products;
    }

    public static PagedResult<ProductEntity> CreatePagedResult(
        List<ProductEntity>? items = null,
        int pageNumber = 1,
        int pageSize = 10,
        int totalCount = 0)
    {
        items ??= CreateProductEntityList(pageSize);
        totalCount = totalCount == 0 ? items.Count : totalCount;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<ProductEntity>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}

