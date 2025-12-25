using Domain.Contractors;

namespace LegacyOrder.Tests.UnitTests.Domain.Entities;

public class OrderItemEntityTests
{
    [Fact]
    public void OrderItemEntity_ImplementsIEntity()
    {
        // Arrange & Act
        var orderItem = new OrderItemEntity();

        // Assert
        orderItem.Should().BeAssignableTo<IEntity<Guid>>();
    }

    [Fact]
    public void OrderItemEntity_CanSetAndGetId()
    {
        // Arrange
        var orderItem = new OrderItemEntity();
        var expectedId = Guid.NewGuid();

        // Act
        orderItem.Id = expectedId;

        // Assert
        orderItem.Id.Should().Be(expectedId);
    }

    [Fact]
    public void OrderItemEntity_CanSetAndGetOrderId()
    {
        // Arrange
        var orderItem = new OrderItemEntity();
        var expectedOrderId = Guid.NewGuid();

        // Act
        orderItem.OrderId = expectedOrderId;

        // Assert
        orderItem.OrderId.Should().Be(expectedOrderId);
    }

    [Fact]
    public void OrderItemEntity_CanSetAndGetProductId()
    {
        // Arrange
        var orderItem = new OrderItemEntity();
        var expectedProductId = Guid.NewGuid();

        // Act
        orderItem.ProductId = expectedProductId;

        // Assert
        orderItem.ProductId.Should().Be(expectedProductId);
    }

    [Fact]
    public void OrderItemEntity_CanSetAndGetQuantity()
    {
        // Arrange
        var orderItem = new OrderItemEntity();
        var expectedQuantity = 5;

        // Act
        orderItem.Quantity = expectedQuantity;

        // Assert
        orderItem.Quantity.Should().Be(expectedQuantity);
    }

    [Fact]
    public void OrderItemEntity_CanSetAndGetUnitPrice()
    {
        // Arrange
        var orderItem = new OrderItemEntity();
        var expectedUnitPrice = 29.99m;

        // Act
        orderItem.UnitPrice = expectedUnitPrice;

        // Assert
        orderItem.UnitPrice.Should().Be(expectedUnitPrice);
    }

    [Fact]
    public void OrderItemEntity_CanSetAndGetLineTotal()
    {
        // Arrange
        var orderItem = new OrderItemEntity();
        var expectedLineTotal = 149.95m;

        // Act
        orderItem.LineTotal = expectedLineTotal;

        // Assert
        orderItem.LineTotal.Should().Be(expectedLineTotal);
    }

    [Fact]
    public void OrderItemEntity_AllPropertiesCanBeSetViaConstructor()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        // Act
        var orderItem = new OrderItemEntity
        {
            Id = itemId,
            OrderId = orderId,
            ProductId = productId,
            Quantity = 3,
            UnitPrice = 50.00m,
            LineTotal = 150.00m
        };

        // Assert
        orderItem.Should().NotBeNull();
        orderItem.Id.Should().Be(itemId);
        orderItem.OrderId.Should().Be(orderId);
        orderItem.ProductId.Should().Be(productId);
        orderItem.Quantity.Should().Be(3);
        orderItem.UnitPrice.Should().Be(50.00m);
        orderItem.LineTotal.Should().Be(150.00m);
    }

    [Fact]
    public void OrderItemEntity_CanSetAndGetOrderNavigation()
    {
        // Arrange
        var orderItem = new OrderItemEntity();
        var order = TestDataBuilder.CreateOrderEntity();

        // Act
        orderItem.Order = order;

        // Assert
        orderItem.Order.Should().NotBeNull();
        orderItem.Order.Should().Be(order);
    }

    [Fact]
    public void OrderItemEntity_CanSetAndGetProductNavigation()
    {
        // Arrange
        var orderItem = new OrderItemEntity();
        var product = TestDataBuilder.CreateProductEntity();

        // Act
        orderItem.Product = product;

        // Assert
        orderItem.Product.Should().NotBeNull();
        orderItem.Product.Should().Be(product);
    }
}

