using Domain.Contractors;

namespace LegacyOrder.Tests.UnitTests.Domain.Entities;

public class ProductEntityTests
{
    [Fact]
    public void ProductEntity_ImplementsIEntity()
    {
        // Arrange & Act
        var product = new ProductEntity();

        // Assert
        product.Should().BeAssignableTo<IEntity<Guid>>();
    }

    [Fact]
    public void ProductEntity_CanSetAndGetId()
    {
        // Arrange
        var product = new ProductEntity();
        var expectedId = Guid.NewGuid();

        // Act
        product.Id = expectedId;

        // Assert
        product.Id.Should().Be(expectedId);
    }

    [Fact]
    public void ProductEntity_CanSetAndGetName()
    {
        // Arrange
        var product = new ProductEntity();
        var expectedName = "Test Product";

        // Act
        product.Name = expectedName;

        // Assert
        product.Name.Should().Be(expectedName);
    }

    [Fact]
    public void ProductEntity_CanSetAndGetDescription()
    {
        // Arrange
        var product = new ProductEntity();
        var expectedDescription = "Test Description";

        // Act
        product.Description = expectedDescription;

        // Assert
        product.Description.Should().Be(expectedDescription);
    }

    [Fact]
    public void ProductEntity_CanSetAndGetSKU()
    {
        // Arrange
        var product = new ProductEntity();
        var expectedSKU = "TEST-SKU-001";

        // Act
        product.SKU = expectedSKU;

        // Assert
        product.SKU.Should().Be(expectedSKU);
    }

    [Fact]
    public void ProductEntity_CanSetAndGetPrice()
    {
        // Arrange
        var product = new ProductEntity();
        var expectedPrice = 99.99m;

        // Act
        product.Price = expectedPrice;

        // Assert
        product.Price.Should().Be(expectedPrice);
    }

    [Fact]
    public void ProductEntity_CanSetAndGetStockQuantity()
    {
        // Arrange
        var product = new ProductEntity();
        var expectedStock = 100;

        // Act
        product.StockQuantity = expectedStock;

        // Assert
        product.StockQuantity.Should().Be(expectedStock);
    }

    [Fact]
    public void ProductEntity_CanSetAndGetCategory()
    {
        // Arrange
        var product = new ProductEntity();
        var expectedCategory = "Electronics";

        // Act
        product.Category = expectedCategory;

        // Assert
        product.Category.Should().Be(expectedCategory);
    }

    [Fact]
    public void ProductEntity_CanSetAndGetIsActive()
    {
        // Arrange
        var product = new ProductEntity();

        // Act
        product.IsActive = true;

        // Assert
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ProductEntity_CanSetAndGetCreatedAt()
    {
        // Arrange
        var product = new ProductEntity();
        var expectedDate = DateTime.UtcNow;

        // Act
        product.CreatedAt = expectedDate;

        // Assert
        product.CreatedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void ProductEntity_CanSetAndGetUpdatedAt()
    {
        // Arrange
        var product = new ProductEntity();
        var expectedDate = DateTime.UtcNow;

        // Act
        product.UpdatedAt = expectedDate;

        // Assert
        product.UpdatedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void ProductEntity_AllPropertiesCanBeSetViaConstructor()
    {
        // Arrange & Act
        var product = TestDataBuilder.CreateProductEntity(
            id: Guid.NewGuid(),
            name: "Test Product",
            description: "Test Description",
            sku: "TEST-SKU",
            price: 99.99m,
            stockQuantity: 50,
            category: "Electronics",
            isActive: true,
            createdAt: DateTime.UtcNow.AddDays(-1),
            updatedAt: DateTime.UtcNow
        );

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be("Test Product");
        product.Description.Should().Be("Test Description");
        product.SKU.Should().Be("TEST-SKU");
        product.Price.Should().Be(99.99m);
        product.StockQuantity.Should().Be(50);
        product.Category.Should().Be("Electronics");
        product.IsActive.Should().BeTrue();
    }
}

