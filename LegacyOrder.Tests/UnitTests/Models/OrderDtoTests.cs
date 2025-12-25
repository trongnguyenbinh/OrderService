namespace LegacyOrder.Tests.UnitTests.Models;

using Model.Models;
using Model.Enums;

public class OrderDtoTests
{
    [Fact]
    public void OrderDto_CanBeCreatedWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var orderNumber = "ORD-001";
        var customerName = "John Doe";
        var customerEmail = "john@example.com";
        var customerType = CustomerType.Premium;
        var orderStatus = OrderStatus.Pending;
        var subTotal = 100m;
        var discountAmount = 10m;
        var totalAmount = 90m;
        var orderDate = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var dto = new OrderDto
        {
            Id = id,
            OrderNumber = orderNumber,
            CustomerId = customerId,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CustomerType = customerType,
            OrderStatus = orderStatus,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            OrderDate = orderDate,
            UpdatedAt = updatedAt
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.OrderNumber.Should().Be(orderNumber);
        dto.CustomerId.Should().Be(customerId);
        dto.CustomerName.Should().Be(customerName);
        dto.CustomerEmail.Should().Be(customerEmail);
        dto.CustomerType.Should().Be(customerType);
        dto.OrderStatus.Should().Be(orderStatus);
        dto.SubTotal.Should().Be(subTotal);
        dto.DiscountAmount.Should().Be(discountAmount);
        dto.TotalAmount.Should().Be(totalAmount);
        dto.OrderDate.Should().Be(orderDate);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void OrderDto_DefaultInitialization_HasDefaultValues()
    {
        // Arrange & Act
        var dto = new OrderDto();

        // Assert
        dto.Id.Should().Be(Guid.Empty);
        dto.OrderNumber.Should().Be(string.Empty);
        dto.CustomerId.Should().Be(Guid.Empty);
        dto.CustomerName.Should().Be(string.Empty);
        dto.CustomerEmail.Should().Be(string.Empty);
        dto.OrderItems.Should().NotBeNull();
        dto.OrderItems.Should().BeEmpty();
    }

    [Fact]
    public void OrderDto_OrderItemsListCanBePopulated()
    {
        // Arrange
        var dto = new OrderDto { Id = Guid.NewGuid() };
        var item1 = new OrderItemDto { Id = Guid.NewGuid(), ProductName = "Product 1" };
        var item2 = new OrderItemDto { Id = Guid.NewGuid(), ProductName = "Product 2" };

        // Act
        dto.OrderItems.Add(item1);
        dto.OrderItems.Add(item2);

        // Assert
        dto.OrderItems.Should().HaveCount(2);
        dto.OrderItems.Should().Contain(item1);
        dto.OrderItems.Should().Contain(item2);
    }

    [Fact]
    public void OrderDto_CanBeModifiedAfterCreation()
    {
        // Arrange
        var dto = new OrderDto { Id = Guid.NewGuid(), OrderStatus = OrderStatus.Pending };
        var newStatus = OrderStatus.Completed;

        // Act
        dto.OrderStatus = newStatus;

        // Assert
        dto.OrderStatus.Should().Be(newStatus);
    }

    [Fact]
    public void OrderDto_MultipleInstances_AreIndependent()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var dto1 = new OrderDto { Id = id1, OrderNumber = "ORD-001" };
        var dto2 = new OrderDto { Id = id2, OrderNumber = "ORD-002" };

        dto1.OrderItems.Add(new OrderItemDto { Id = Guid.NewGuid() });

        // Assert
        dto1.Id.Should().NotBe(dto2.Id);
        dto1.OrderNumber.Should().NotBe(dto2.OrderNumber);
        dto1.OrderItems.Should().HaveCount(1);
        dto2.OrderItems.Should().BeEmpty();
    }
}

