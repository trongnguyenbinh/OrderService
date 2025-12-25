using Domain.Contractors;
using LegacyOrder.Tests.TestFixtures;
using Model.Enums;

namespace LegacyOrder.Tests.UnitTests.Domain.Entities;

public class OrderEntityTests
{
    [Fact]
    public void OrderEntity_ImplementsIEntity()
    {
        // Arrange & Act
        var order = new OrderEntity();

        // Assert
        order.Should().BeAssignableTo<IEntity<Guid>>();
    }

    [Fact]
    public void OrderEntity_CanSetAndGetId()
    {
        // Arrange
        var order = new OrderEntity();
        var expectedId = Guid.NewGuid();

        // Act
        order.Id = expectedId;

        // Assert
        order.Id.Should().Be(expectedId);
    }

    [Fact]
    public void OrderEntity_CanSetAndGetOrderNumber()
    {
        // Arrange
        var order = new OrderEntity();
        var expectedOrderNumber = "ORD-20240101120000-1234";

        // Act
        order.OrderNumber = expectedOrderNumber;

        // Assert
        order.OrderNumber.Should().Be(expectedOrderNumber);
    }

    [Fact]
    public void OrderEntity_CanSetAndGetCustomerId()
    {
        // Arrange
        var order = new OrderEntity();
        var expectedCustomerId = Guid.NewGuid();

        // Act
        order.CustomerId = expectedCustomerId;

        // Assert
        order.CustomerId.Should().Be(expectedCustomerId);
    }

    [Fact]
    public void OrderEntity_CanSetAndGetOrderStatus()
    {
        // Arrange
        var order = new OrderEntity();
        var expectedStatus = OrderStatus.Completed;

        // Act
        order.OrderStatus = expectedStatus;

        // Assert
        order.OrderStatus.Should().Be(expectedStatus);
    }

    [Fact]
    public void OrderEntity_DefaultOrderStatusIsPending()
    {
        // Arrange & Act
        var order = new OrderEntity();

        // Assert
        order.OrderStatus.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void OrderEntity_CanSetAndGetSubTotal()
    {
        // Arrange
        var order = new OrderEntity();
        var expectedSubTotal = 150.50m;

        // Act
        order.SubTotal = expectedSubTotal;

        // Assert
        order.SubTotal.Should().Be(expectedSubTotal);
    }

    [Fact]
    public void OrderEntity_CanSetAndGetDiscountAmount()
    {
        // Arrange
        var order = new OrderEntity();
        var expectedDiscount = 15.05m;

        // Act
        order.DiscountAmount = expectedDiscount;

        // Assert
        order.DiscountAmount.Should().Be(expectedDiscount);
    }

    [Fact]
    public void OrderEntity_CanSetAndGetTotalAmount()
    {
        // Arrange
        var order = new OrderEntity();
        var expectedTotal = 135.45m;

        // Act
        order.TotalAmount = expectedTotal;

        // Assert
        order.TotalAmount.Should().Be(expectedTotal);
    }

    [Fact]
    public void OrderEntity_CanSetAndGetOrderDate()
    {
        // Arrange
        var order = new OrderEntity();
        var expectedDate = DateTime.UtcNow.AddDays(-1);

        // Act
        order.OrderDate = expectedDate;

        // Assert
        order.OrderDate.Should().Be(expectedDate);
    }

    [Fact]
    public void OrderEntity_CanSetAndGetUpdatedAt()
    {
        // Arrange
        var order = new OrderEntity();
        var expectedDate = DateTime.UtcNow;

        // Act
        order.UpdatedAt = expectedDate;

        // Assert
        order.UpdatedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void OrderEntity_OrderItemsCollectionIsInitialized()
    {
        // Arrange & Act
        var order = new OrderEntity();

        // Assert
        order.OrderItems.Should().NotBeNull();
        order.OrderItems.Should().BeEmpty();
    }
}

