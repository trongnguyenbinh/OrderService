using Domain;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.Tests.UnitTests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly DataContext _context;
    private readonly ProductRepository _repository;
    private readonly Mock<ILogger<ProductRepository>> _mockLogger;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mockLogger = LoggerFixture.CreateLogger<ProductRepository>();
        _repository = new ProductRepository(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var product = TestDataBuilder.CreateProductEntity();
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.SKU.Should().Be(product.SKU);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_UsesAsNoTracking()
    {
        // Arrange
        var product = TestDataBuilder.CreateProductEntity();
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        var trackedEntities = _context.ChangeTracker.Entries<ProductEntity>().ToList();
        trackedEntities.Should().BeEmpty();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ReturnsAllProducts()
    {
        // Arrange
        var products = TestDataBuilder.CreateProductEntityList(5);
        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoProducts_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(name: "Product 1", createdAt: DateTime.UtcNow.AddDays(-2));
        var product2 = TestDataBuilder.CreateProductEntity(name: "Product 2", createdAt: DateTime.UtcNow.AddDays(-1));
        var product3 = TestDataBuilder.CreateProductEntity(name: "Product 3", createdAt: DateTime.UtcNow);

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Product 3");
        result[1].Name.Should().Be("Product 2");
        result[2].Name.Should().Be("Product 1");
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidEntity_GeneratesNewGuid()
    {
        // Arrange
        var product = TestDataBuilder.CreateProductEntity(id: Guid.Empty);

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_SetsCreatedAtTimestamp()
    {
        // Arrange
        var beforeAdd = DateTime.UtcNow;
        var product = TestDataBuilder.CreateProductEntity();

        // Act
        var result = await _repository.AddAsync(product);
        var afterAdd = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeOnOrAfter(beforeAdd);
        result.CreatedAt.Should().BeOnOrBefore(afterAdd);
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_SetsUpdatedAtTimestamp()
    {
        // Arrange
        var beforeAdd = DateTime.UtcNow;
        var product = TestDataBuilder.CreateProductEntity();

        // Act
        var result = await _repository.AddAsync(product);
        var afterAdd = DateTime.UtcNow;

        // Assert
        result.UpdatedAt.Should().BeOnOrAfter(beforeAdd);
        result.UpdatedAt.Should().BeOnOrBefore(afterAdd);
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_SavesProductToDatabase()
    {
        // Arrange
        var product = TestDataBuilder.CreateProductEntity();

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        var savedProduct = await _context.Products.FindAsync(result.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be(product.Name);
        savedProduct.SKU.Should().Be(product.SKU);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenProductExists_UpdatesAllProperties()
    {
        // Arrange
        var product = TestDataBuilder.CreateProductEntity(
            name: "Original Name",
            description: "Original Description",
            sku: "ORIGINAL-SKU",
            price: 50m,
            stockQuantity: 5,
            category: "Original Category",
            isActive: true
        );
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var updatedProduct = TestDataBuilder.CreateProductEntity(
            id: product.Id,
            name: "Updated Name",
            description: "Updated Description",
            sku: "UPDATED-SKU",
            price: 100m,
            stockQuantity: 10,
            category: "Updated Category",
            isActive: false
        );

        // Act
        var result = await _repository.UpdateAsync(updatedProduct);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.SKU.Should().Be("UPDATED-SKU");
        result.Price.Should().Be(100m);
        result.StockQuantity.Should().Be(10);
        result.Category.Should().Be("Updated Category");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var product = TestDataBuilder.CreateProductEntity(updatedAt: DateTime.UtcNow.AddDays(-1));
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var beforeUpdate = DateTime.UtcNow;
        var updatedProduct = TestDataBuilder.CreateProductEntity(id: product.Id, name: "Updated Name");

        // Act
        var result = await _repository.UpdateAsync(updatedProduct);
        var afterUpdate = DateTime.UtcNow;

        // Assert
        result.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
        result.UpdatedAt.Should().BeOnOrBefore(afterUpdate);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentProduct = TestDataBuilder.CreateProductEntity(id: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _repository.UpdateAsync(nonExistentProduct);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Product with ID {nonExistentProduct.Id} not found");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenProductExists_PerformsSoftDelete()
    {
        // Arrange
        var product = TestDataBuilder.CreateProductEntity(isActive: true);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.DeleteAsync(product.Id);

        // Assert
        result.Should().BeTrue();
        var deletedProduct = await _context.Products.FindAsync(product.Id);
        deletedProduct.Should().NotBeNull();
        deletedProduct!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var product = TestDataBuilder.CreateProductEntity(updatedAt: DateTime.UtcNow.AddDays(-1));
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var beforeDelete = DateTime.UtcNow;

        // Act
        var result = await _repository.DeleteAsync(product.Id);
        var afterDelete = DateTime.UtcNow;

        // Assert
        result.Should().BeTrue();
        var deletedProduct = await _context.Products.FindAsync(product.Id);
        deletedProduct!.UpdatedAt.Should().BeOnOrAfter(beforeDelete);
        deletedProduct.UpdatedAt.Should().BeOnOrBefore(afterDelete);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductNotFound_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetBySkuAsync Tests

    [Fact]
    public async Task GetBySkuAsync_WhenSkuExists_ReturnsProduct()
    {
        // Arrange
        var product = TestDataBuilder.CreateProductEntity(sku: "TEST-SKU-123");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySkuAsync("TEST-SKU-123");

        // Assert
        result.Should().NotBeNull();
        result!.SKU.Should().Be("TEST-SKU-123");
    }

    [Fact]
    public async Task GetBySkuAsync_WhenSkuNotFound_ReturnsNull()
    {
        // Act
        var result = await _repository.GetBySkuAsync("NON-EXISTENT-SKU");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_WithNoFilters_ReturnsAllActiveProducts()
    {
        // Arrange
        var activeProducts = TestDataBuilder.CreateProductEntityList(3);
        var inactiveProduct = TestDataBuilder.CreateProductEntity(name: "Inactive Product", isActive: false);

        await _context.Products.AddRangeAsync(activeProducts);
        await _context.Products.AddAsync(inactiveProduct);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest();

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.Items.Should().OnlyContain(p => p.IsActive);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task SearchAsync_FilterByName_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(name: "Laptop Computer");
        var product2 = TestDataBuilder.CreateProductEntity(name: "Desktop Computer");
        var product3 = TestDataBuilder.CreateProductEntity(name: "Mobile Phone");

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(name: "Computer");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Name == "Laptop Computer");
        result.Items.Should().Contain(p => p.Name == "Desktop Computer");
    }

    [Fact]
    public async Task SearchAsync_FilterByName_IsCaseInsensitive()
    {
        // Arrange
        var product = TestDataBuilder.CreateProductEntity(name: "Laptop Computer");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(name: "laptop");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Laptop Computer");
    }

    [Fact]
    public async Task SearchAsync_FilterByDescription_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(name: "Product 1", description: "High quality product");
        var product2 = TestDataBuilder.CreateProductEntity(name: "Product 2", description: "Budget friendly");

        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(description: "quality");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Product 1");
    }

    [Fact]
    public async Task SearchAsync_FilterBySku_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(sku: "LAPTOP-001");
        var product2 = TestDataBuilder.CreateProductEntity(sku: "PHONE-001");

        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(sku: "LAPTOP");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().SKU.Should().Be("LAPTOP-001");
    }

    [Fact]
    public async Task SearchAsync_FilterByCategory_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(category: "Electronics");
        var product2 = TestDataBuilder.CreateProductEntity(category: "Books");

        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(category: "Electronics");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Category.Should().Be("Electronics");
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var products = TestDataBuilder.CreateProductEntityList(15);
        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(pageNumber: 2, pageSize: 5);

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task SearchAsync_SortByPriceAscending_ReturnsSortedResults()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(name: "Product 1", price: 100m);
        var product2 = TestDataBuilder.CreateProductEntity(name: "Product 2", price: 50m);
        var product3 = TestDataBuilder.CreateProductEntity(name: "Product 3", price: 75m);

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(sortBy: "price", sortDirection: "asc");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(3);
        var items = result.Items.ToList();
        items[0].Price.Should().Be(50m);
        items[1].Price.Should().Be(75m);
        items[2].Price.Should().Be(100m);
    }

    [Fact]
    public async Task SearchAsync_SortByPriceDescending_ReturnsSortedResults()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(name: "Product 1", price: 100m);
        var product2 = TestDataBuilder.CreateProductEntity(name: "Product 2", price: 50m);
        var product3 = TestDataBuilder.CreateProductEntity(name: "Product 3", price: 75m);

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(sortBy: "price", sortDirection: "desc");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        var items = result.Items.ToList();
        items[0].Price.Should().Be(100m);
        items[1].Price.Should().Be(75m);
        items[2].Price.Should().Be(50m);
    }

    [Fact]
    public async Task SearchAsync_SortByStockQuantityAscending_ReturnsSortedResults()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(name: "Product 1", stockQuantity: 100);
        var product2 = TestDataBuilder.CreateProductEntity(name: "Product 2", stockQuantity: 50);
        var product3 = TestDataBuilder.CreateProductEntity(name: "Product 3", stockQuantity: 75);

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(sortBy: "stockquantity", sortDirection: "asc");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        var items = result.Items.ToList();
        items[0].StockQuantity.Should().Be(50);
        items[1].StockQuantity.Should().Be(75);
        items[2].StockQuantity.Should().Be(100);
    }

    [Fact]
    public async Task SearchAsync_SortByStockQuantityDescending_ReturnsSortedResults()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(name: "Product 1", stockQuantity: 100);
        var product2 = TestDataBuilder.CreateProductEntity(name: "Product 2", stockQuantity: 50);
        var product3 = TestDataBuilder.CreateProductEntity(name: "Product 3", stockQuantity: 75);

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(sortBy: "stockquantity", sortDirection: "desc");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        var items = result.Items.ToList();
        items[0].StockQuantity.Should().Be(100);
        items[1].StockQuantity.Should().Be(75);
        items[2].StockQuantity.Should().Be(50);
    }

    [Fact]
    public async Task SearchAsync_DefaultSort_OrdersByCreatedAtDescending()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(name: "Product 1", createdAt: DateTime.UtcNow.AddDays(-2));
        var product2 = TestDataBuilder.CreateProductEntity(name: "Product 2", createdAt: DateTime.UtcNow.AddDays(-1));
        var product3 = TestDataBuilder.CreateProductEntity(name: "Product 3", createdAt: DateTime.UtcNow);

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest();

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        var items = result.Items.ToList();
        items[0].Name.Should().Be("Product 3");
        items[1].Name.Should().Be("Product 2");
        items[2].Name.Should().Be("Product 1");
    }

    [Fact]
    public async Task SearchAsync_CalculatesTotalPagesCorrectly()
    {
        // Arrange
        var products = TestDataBuilder.CreateProductEntityList(23);
        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(pageSize: 10);

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.TotalCount.Should().Be(23);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task SearchAsync_WithMultipleFilters_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = TestDataBuilder.CreateProductEntity(
            name: "Gaming Laptop",
            category: "Electronics",
            sku: "LAPTOP-001"
        );
        var product2 = TestDataBuilder.CreateProductEntity(
            name: "Gaming Mouse",
            category: "Electronics",
            sku: "MOUSE-001"
        );
        var product3 = TestDataBuilder.CreateProductEntity(
            name: "Office Laptop",
            category: "Electronics",
            sku: "LAPTOP-002"
        );

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateSearchRequest(
            name: "Laptop",
            category: "Electronics",
            sku: "LAPTOP"
        );

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(p => p.Name == "Gaming Laptop");
        result.Items.Should().Contain(p => p.Name == "Office Laptop");
    }

    #endregion
}




