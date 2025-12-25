namespace LegacyOrder.Tests.UnitTests.Models;

using Model.Models;

public class ProductDtoTests
{
    [Fact]
    public void ProductDto_CanBeCreatedWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Product";
        var description = "A test product";
        var sku = "SKU-001";
        var price = 99.99m;
        var stockQuantity = 100;
        var category = "Electronics";
        var isActive = true;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var dto = new ProductDto
        {
            Id = id,
            Name = name,
            Description = description,
            SKU = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.Name.Should().Be(name);
        dto.Description.Should().Be(description);
        dto.SKU.Should().Be(sku);
        dto.Price.Should().Be(price);
        dto.StockQuantity.Should().Be(stockQuantity);
        dto.Category.Should().Be(category);
        dto.IsActive.Should().BeTrue();
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ProductDto_DefaultInitialization_HasDefaultValues()
    {
        // Arrange & Act
        var dto = new ProductDto();

        // Assert
        dto.Id.Should().Be(Guid.Empty);
        dto.Name.Should().Be(string.Empty);
        dto.Description.Should().BeNull();
        dto.SKU.Should().Be(string.Empty);
        dto.Price.Should().Be(0m);
        dto.StockQuantity.Should().Be(0);
        dto.Category.Should().BeNull();
        dto.IsActive.Should().BeFalse();
        dto.CreatedAt.Should().Be(default(DateTime));
        dto.UpdatedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void ProductDto_CanBeModifiedAfterCreation()
    {
        // Arrange
        var dto = new ProductDto { Id = Guid.NewGuid(), Name = "Original" };
        var newName = "Updated Product";
        var newPrice = 149.99m;

        // Act
        dto.Name = newName;
        dto.Price = newPrice;

        // Assert
        dto.Name.Should().Be(newName);
        dto.Price.Should().Be(newPrice);
    }

    [Fact]
    public void ProductDto_WithNullableProperties_CanBeNull()
    {
        // Arrange & Act
        var dto = new ProductDto
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            SKU = "SKU-001",
            Description = null,
            Category = null
        };

        // Assert
        dto.Description.Should().BeNull();
        dto.Category.Should().BeNull();
    }

    [Fact]
    public void ProductDto_MultipleInstances_AreIndependent()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var dto1 = new ProductDto { Id = id1, Name = "Product 1", Price = 50m };
        var dto2 = new ProductDto { Id = id2, Name = "Product 2", Price = 100m };

        // Assert
        dto1.Id.Should().NotBe(dto2.Id);
        dto1.Name.Should().NotBe(dto2.Name);
        dto1.Price.Should().NotBe(dto2.Price);
    }
}

