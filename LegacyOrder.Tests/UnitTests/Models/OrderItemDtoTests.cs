namespace LegacyOrder.Tests.UnitTests.Models;

using Model.Models;

public class OrderItemDtoTests
{
    [Fact]
    public void OrderItemDto_CanBeCreatedWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var productSKU = "SKU-001";
        var quantity = 5;
        var unitPrice = 99.99m;
        var lineTotal = 499.95m;

        // Act
        var dto = new OrderItemDto
        {
            Id = id,
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            ProductSKU = productSKU,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = lineTotal
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.OrderId.Should().Be(orderId);
        dto.ProductId.Should().Be(productId);
        dto.ProductName.Should().Be(productName);
        dto.ProductSKU.Should().Be(productSKU);
        dto.Quantity.Should().Be(quantity);
        dto.UnitPrice.Should().Be(unitPrice);
        dto.LineTotal.Should().Be(lineTotal);
    }

    [Fact]
    public void OrderItemDto_DefaultInitialization_HasDefaultValues()
    {
        // Arrange & Act
        var dto = new OrderItemDto();

        // Assert
        dto.Id.Should().Be(Guid.Empty);
        dto.OrderId.Should().Be(Guid.Empty);
        dto.ProductId.Should().Be(Guid.Empty);
        dto.ProductName.Should().Be(string.Empty);
        dto.ProductSKU.Should().Be(string.Empty);
        dto.Quantity.Should().Be(0);
        dto.UnitPrice.Should().Be(0m);
        dto.LineTotal.Should().Be(0m);
    }

    [Fact]
    public void OrderItemDto_CanBeModifiedAfterCreation()
    {
        // Arrange
        var dto = new OrderItemDto { Id = Guid.NewGuid(), Quantity = 1 };
        var newQuantity = 10;
        var newLineTotal = 999.90m;

        // Act
        dto.Quantity = newQuantity;
        dto.LineTotal = newLineTotal;

        // Assert
        dto.Quantity.Should().Be(newQuantity);
        dto.LineTotal.Should().Be(newLineTotal);
    }

    [Fact]
    public void OrderItemDto_MultipleInstances_AreIndependent()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var dto1 = new OrderItemDto { Id = id1, ProductName = "Product 1", Quantity = 5 };
        var dto2 = new OrderItemDto { Id = id2, ProductName = "Product 2", Quantity = 10 };

        // Assert
        dto1.Id.Should().NotBe(dto2.Id);
        dto1.ProductName.Should().NotBe(dto2.ProductName);
        dto1.Quantity.Should().NotBe(dto2.Quantity);
    }
}

