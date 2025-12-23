using Domain;
using LegacyOrder.Tests.TestFixtures;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.Tests.IntegrationTests;

public class ProductFlowIntegrationTests : IDisposable
{
    private readonly DataContext _context;
    private readonly ProductRepository _repository;
    private readonly ProductService _service;
    private readonly IMapper _mapper;

    public ProductFlowIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mapper = AutoMapperFixture.CreateMapper();
        
        var repositoryLogger = LoggerFixture.CreateNullLogger<ProductRepository>();
        _repository = new ProductRepository(_context, repositoryLogger);
        
        var serviceLogger = LoggerFixture.CreateNullLogger<ProductService>();
        _service = new ProductService(_repository, serviceLogger, _mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Create-Read-Update-Delete Flow Tests

    [Fact]
    public async Task ProductFlow_CreateAndRetrieve_Success()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateProductRequest(
            name: "Integration Test Product",
            sku: "INT-TEST-001",
            price: 199.99m,
            stockQuantity: 100
        );

        // Act - Create
        var createdProduct = await _service.CreateAsync(createRequest);

        // Act - Retrieve
        var retrievedProduct = await _service.GetByIdAsync(createdProduct.Id);

        // Assert
        retrievedProduct.Should().NotBeNull();
        retrievedProduct!.Id.Should().Be(createdProduct.Id);
        retrievedProduct.Name.Should().Be(createRequest.Name);
        retrievedProduct.SKU.Should().Be(createRequest.SKU);
        retrievedProduct.Price.Should().Be(createRequest.Price);
        retrievedProduct.StockQuantity.Should().Be(createRequest.StockQuantity);
    }

    [Fact]
    public async Task ProductFlow_CreateUpdateAndRetrieve_Success()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateProductRequest(
            name: "Original Product",
            sku: "ORIG-001",
            price: 99.99m
        );

        var updateRequest = TestDataBuilder.CreateUpdateProductRequest(
            name: "Updated Product",
            sku: "UPDATED-001",
            price: 149.99m
        );

        // Act - Create
        var createdProduct = await _service.CreateAsync(createRequest);

        // Act - Update
        var updatedProduct = await _service.UpdateAsync(createdProduct.Id, updateRequest);

        // Act - Retrieve
        var retrievedProduct = await _service.GetByIdAsync(createdProduct.Id);

        // Assert
        retrievedProduct.Should().NotBeNull();
        retrievedProduct!.Name.Should().Be(updateRequest.Name);
        retrievedProduct.SKU.Should().Be(updateRequest.SKU);
        retrievedProduct.Price.Should().Be(updateRequest.Price);
    }

    [Fact]
    public async Task ProductFlow_CreateDeleteAndRetrieve_ProductNotActive()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateProductRequest(
            name: "Product To Delete",
            sku: "DELETE-001"
        );

        // Act - Create
        var createdProduct = await _service.CreateAsync(createRequest);

        // Act - Delete (soft delete)
        var deleteResult = await _service.DeleteAsync(createdProduct.Id);

        // Act - Retrieve from repository directly to check soft delete
        var deletedEntity = await _repository.GetByIdAsync(createdProduct.Id);

        // Assert
        deleteResult.Should().BeTrue();
        deletedEntity.Should().NotBeNull();
        deletedEntity!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ProductFlow_CreateMultipleAndSearch_ReturnsFilteredResults()
    {
        // Arrange
        var products = new[]
        {
            TestDataBuilder.CreateProductRequest(name: "Laptop Pro", sku: "LAPTOP-001", category: "Electronics", price: 1299.99m),
            TestDataBuilder.CreateProductRequest(name: "Laptop Air", sku: "LAPTOP-002", category: "Electronics", price: 999.99m),
            TestDataBuilder.CreateProductRequest(name: "Mouse Wireless", sku: "MOUSE-001", category: "Electronics", price: 29.99m),
            TestDataBuilder.CreateProductRequest(name: "Book: Clean Code", sku: "BOOK-001", category: "Books", price: 39.99m)
        };

        foreach (var product in products)
        {
            await _service.CreateAsync(product);
        }

        var searchRequest = TestDataBuilder.CreateSearchRequest(
            name: "Laptop",
            category: "Electronics"
        );

        // Act
        var searchResult = await _service.SearchAsync(searchRequest);

        // Assert
        searchResult.Should().NotBeNull();
        searchResult.Items.Should().HaveCount(2);
        searchResult.Items.Should().OnlyContain(p => p.Name!.Contains("Laptop"));
        searchResult.TotalCount.Should().Be(2);
    }

    #endregion

    #region Pagination and Sorting Tests

    [Fact]
    public async Task ProductFlow_SearchWithPagination_ReturnsCorrectPage()
    {
        // Arrange - Create 15 products
        for (int i = 1; i <= 15; i++)
        {
            var request = TestDataBuilder.CreateProductRequest(
                name: $"Product {i}",
                sku: $"SKU-{i:D3}",
                price: i * 10m
            );
            await _service.CreateAsync(request);
        }

        var searchRequest = TestDataBuilder.CreateSearchRequest(
            pageNumber: 2,
            pageSize: 5
        );

        // Act
        var result = await _service.SearchAsync(searchRequest);

        // Assert
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task ProductFlow_SearchWithSortByPrice_ReturnsSortedResults()
    {
        // Arrange
        var products = new[]
        {
            TestDataBuilder.CreateProductRequest(name: "Product A", sku: "SKU-A", price: 100m),
            TestDataBuilder.CreateProductRequest(name: "Product B", sku: "SKU-B", price: 50m),
            TestDataBuilder.CreateProductRequest(name: "Product C", sku: "SKU-C", price: 75m)
        };

        foreach (var product in products)
        {
            await _service.CreateAsync(product);
        }

        var searchRequest = TestDataBuilder.CreateSearchRequest(
            sortBy: "price",
            sortDirection: "asc"
        );

        // Act
        var result = await _service.SearchAsync(searchRequest);

        // Assert
        var items = result.Items.ToList();
        items[0].Price.Should().Be(50m);
        items[1].Price.Should().Be(75m);
        items[2].Price.Should().Be(100m);
    }

    #endregion

    #region Business Rule Validation Tests

    [Fact]
    public async Task ProductFlow_CreateWithDuplicateSku_ThrowsInvalidOperationException()
    {
        // Arrange
        var request1 = TestDataBuilder.CreateProductRequest(sku: "DUPLICATE-SKU");
        var request2 = TestDataBuilder.CreateProductRequest(sku: "DUPLICATE-SKU");

        await _service.CreateAsync(request1);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request2);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*DUPLICATE-SKU*already exists*");
    }

    [Fact]
    public async Task ProductFlow_UpdateWithDuplicateSku_ThrowsInvalidOperationException()
    {
        // Arrange
        var product1 = await _service.CreateAsync(
            TestDataBuilder.CreateProductRequest(sku: "SKU-001")
        );
        var product2 = await _service.CreateAsync(
            TestDataBuilder.CreateProductRequest(sku: "SKU-002")
        );

        var updateRequest = TestDataBuilder.CreateUpdateProductRequest(sku: "SKU-001");

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(product2.Id, updateRequest);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*SKU-001*already used by another product*");
    }

    [Fact]
    public async Task ProductFlow_CreateWithInvalidPrice_ThrowsArgumentException()
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest(price: -10m);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*price must be greater than zero*");
    }

    [Fact]
    public async Task ProductFlow_UpdateNonExistentProduct_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = TestDataBuilder.CreateUpdateProductRequest();

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(nonExistentId, updateRequest);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{nonExistentId}*not found*");
    }

    #endregion

    #region Search Filter Tests

    [Fact]
    public async Task ProductFlow_SearchBySkuPartial_ReturnsMatchingProducts()
    {
        // Arrange
        await _service.CreateAsync(TestDataBuilder.CreateProductRequest(sku: "LAPTOP-001"));
        await _service.CreateAsync(TestDataBuilder.CreateProductRequest(sku: "LAPTOP-002"));
        await _service.CreateAsync(TestDataBuilder.CreateProductRequest(sku: "PHONE-001"));

        var searchRequest = TestDataBuilder.CreateSearchRequest(sku: "LAPTOP");

        // Act
        var result = await _service.SearchAsync(searchRequest);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(p => p.SKU.Contains("LAPTOP"));
    }

    [Fact]
    public async Task ProductFlow_SearchExcludesInactiveProducts()
    {
        // Arrange
        var activeProduct = await _service.CreateAsync(
            TestDataBuilder.CreateProductRequest(name: "Active Product", sku: "ACTIVE-001")
        );
        var inactiveProduct = await _service.CreateAsync(
            TestDataBuilder.CreateProductRequest(name: "Inactive Product", sku: "INACTIVE-001")
        );

        // Soft delete the inactive product
        await _service.DeleteAsync(inactiveProduct.Id);

        var searchRequest = TestDataBuilder.CreateSearchRequest();

        // Act
        var result = await _service.SearchAsync(searchRequest);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.Should().OnlyContain(p => p.Id == activeProduct.Id);
    }

    #endregion

    #region GetBySku Tests

    [Fact]
    public async Task ProductFlow_GetBySkuAfterCreate_ReturnsProduct()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateProductRequest(sku: "UNIQUE-SKU-001");
        await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.GetBySkuAsync("UNIQUE-SKU-001");

        // Assert
        result.Should().NotBeNull();
        result!.SKU.Should().Be("UNIQUE-SKU-001");
    }

    [Fact]
    public async Task ProductFlow_GetBySkuNonExistent_ReturnsNull()
    {
        // Act
        var result = await _service.GetBySkuAsync("NON-EXISTENT-SKU");

        // Assert
        result.Should().BeNull();
    }

    #endregion
}


