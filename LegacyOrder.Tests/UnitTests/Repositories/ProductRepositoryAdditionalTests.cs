using Domain;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.Tests.UnitTests.Repositories;

public class ProductRepositoryAdditionalTests
{
    private readonly DataContext _context;
    private readonly Mock<ILogger<ProductRepository>> _mockLogger;
    private readonly ProductRepository _repository;

    public ProductRepositoryAdditionalTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mockLogger = new Mock<ILogger<ProductRepository>>();
        _repository = new ProductRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetBySkuAsync_WithExistingSku_ReturnsProduct()
    {
        // Arrange
        var product = new ProductEntity { Name = "Test Product", SKU = "SKU-001", Price = 99.99m, IsActive = true };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySkuAsync("SKU-001");

        // Assert
        result.Should().NotBeNull();
        result!.SKU.Should().Be("SKU-001");
    }

    [Fact]
    public async Task GetBySkuAsync_WithNonExistentSku_ReturnsNull()
    {
        // Act
        var result = await _repository.GetBySkuAsync("NONEXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchForAIToolAsync_WithNameFilter_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = new ProductEntity { Name = "Laptop", SKU = "SKU-001", Price = 999.99m, IsActive = true };
        var product2 = new ProductEntity { Name = "Mouse", SKU = "SKU-002", Price = 29.99m, IsActive = true };
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        var request = new ProductSearchRequest { Name = "Laptop", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _repository.SearchForAIToolAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task SearchForAIToolAsync_WithCategoryFilter_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = new ProductEntity { Name = "Laptop", SKU = "SKU-001", Category = "Electronics", Price = 999.99m, IsActive = true };
        var product2 = new ProductEntity { Name = "Desk", SKU = "SKU-002", Category = "Furniture", Price = 299.99m, IsActive = true };
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        var request = new ProductSearchRequest { Category = "Electronics", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _repository.SearchForAIToolAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Category.Should().Be("Electronics");
    }

    [Fact]
    public async Task SearchAsync_WithNameFilter_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = new ProductEntity { Name = "Laptop", SKU = "SKU-001", Price = 999.99m, IsActive = true };
        var product2 = new ProductEntity { Name = "Mouse", SKU = "SKU-002", Price = 29.99m, IsActive = true };
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        var request = new ProductSearchRequest { Name = "Laptop", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task SearchAsync_WithSortByPrice_ReturnsSortedResults()
    {
        // Arrange
        var product1 = new ProductEntity { Name = "Expensive", SKU = "SKU-001", Price = 999.99m, IsActive = true };
        var product2 = new ProductEntity { Name = "Cheap", SKU = "SKU-002", Price = 29.99m, IsActive = true };
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        var request = new ProductSearchRequest { SortBy = "price", SortDirection = "asc", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.First().Price.Should().Be(29.99m);
    }

    [Fact]
    public async Task GetByIdsAsync_WithExistingIds_ReturnsProducts()
    {
        // Arrange
        var product1 = new ProductEntity { Name = "Product 1", SKU = "SKU-001", Price = 99.99m, IsActive = true };
        var product2 = new ProductEntity { Name = "Product 2", SKU = "SKU-002", Price = 199.99m, IsActive = true };
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        var ids = new[] { product1.Id, product2.Id };

        // Act
        var result = await _repository.GetByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateRangeAsync_WithMultipleProducts_UpdatesAll()
    {
        // Arrange
        var product1 = new ProductEntity { Name = "Product 1", SKU = "SKU-001", Price = 99.99m, IsActive = true };
        var product2 = new ProductEntity { Name = "Product 2", SKU = "SKU-002", Price = 199.99m, IsActive = true };
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        product1.Price = 149.99m;
        product2.Price = 249.99m;

        // Act
        await _repository.UpdateRangeAsync(new[] { product1, product2 });

        // Assert
        var updated1 = await _context.Products.FindAsync(product1.Id);
        var updated2 = await _context.Products.FindAsync(product2.Id);
        updated1!.Price.Should().Be(149.99m);
        updated2!.Price.Should().Be(249.99m);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentProduct_ThrowsKeyNotFoundException()
    {
        // Arrange
        var product = new ProductEntity { Id = Guid.NewGuid(), Name = "Test", SKU = "SKU-001", Price = 99.99m };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(product));
    }
}

