using Domain;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.Tests.UnitTests.Repositories;

public class OrderRepositoryTests : IDisposable
{
    private readonly DataContext _context;
    private readonly OrderRepository _repository;
    private readonly Mock<ILogger<OrderRepository>> _mockLogger;

    public OrderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mockLogger = LoggerFixture.CreateLogger<OrderRepository>();
        _repository = new OrderRepository(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ReturnsOrder()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var order = TestDataBuilder.CreateOrderEntity(customerId: customer.Id);
        await _context.Customers.AddAsync(customer);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.OrderNumber.Should().Be(order.OrderNumber);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderNotFound_ReturnsNull()
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
        var customer = TestDataBuilder.CreateCustomerEntity();
        var order = TestDataBuilder.CreateOrderEntity(customerId: customer.Id);
        await _context.Customers.AddAsync(customer);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        var trackedEntities = _context.ChangeTracker.Entries<OrderEntity>().ToList();
        trackedEntities.Should().BeEmpty();
    }

    #endregion

    #region GetByOrderNumberAsync Tests

    [Fact]
    public async Task GetByOrderNumberAsync_WhenOrderExists_ReturnsOrder()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var orderNumber = "ORD-20240101120000-1234";
        var order = TestDataBuilder.CreateOrderEntity(customerId: customer.Id, orderNumber: orderNumber);
        await _context.Customers.AddAsync(customer);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByOrderNumberAsync(orderNumber);

        // Assert
        result.Should().NotBeNull();
        result!.OrderNumber.Should().Be(orderNumber);
        result.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_WhenOrderNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentOrderNumber = "ORD-NONEXISTENT";

        // Act
        var result = await _repository.GetByOrderNumberAsync(nonExistentOrderNumber);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdWithDetailsAsync Tests

    [Fact]
    public async Task GetByIdWithDetailsAsync_WhenOrderExists_ReturnsOrderWithDetails()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var product = TestDataBuilder.CreateProductEntity();
        var order = TestDataBuilder.CreateOrderEntity(customerId: customer.Id);
        var orderItem = TestDataBuilder.CreateOrderItemEntity(orderId: order.Id, productId: product.Id);
        
        await _context.Customers.AddAsync(customer);
        await _context.Products.AddAsync(product);
        await _context.Orders.AddAsync(order);
        await _context.OrderItems.AddAsync(orderItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Customer.Should().NotBeNull();
        result.OrderItems.Should().HaveCount(1);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenOrdersExist_ReturnsAllOrders()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var orders = TestDataBuilder.CreateOrderEntityList(3, customerId: customer.Id);
        await _context.Customers.AddAsync(customer);
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoOrders_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_OrdersByOrderDateDescending()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var order1 = TestDataBuilder.CreateOrderEntity(customerId: customer.Id, orderDate: DateTime.UtcNow.AddDays(-2));
        var order2 = TestDataBuilder.CreateOrderEntity(customerId: customer.Id, orderDate: DateTime.UtcNow.AddDays(-1));
        var order3 = TestDataBuilder.CreateOrderEntity(customerId: customer.Id, orderDate: DateTime.UtcNow);

        await _context.Customers.AddAsync(customer);
        await _context.Orders.AddRangeAsync(order1, order2, order3);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].OrderDate.Should().BeOnOrAfter(result[1].OrderDate);
        result[1].OrderDate.Should().BeOnOrAfter(result[2].OrderDate);
    }

    #endregion

    #region GetAllWithDetailsAsync Tests

    [Fact]
    public async Task GetAllWithDetailsAsync_WithValidParameters_ReturnsPagedOrders()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var orders = TestDataBuilder.CreateOrderEntityList(5, customerId: customer.Id);
        await _context.Customers.AddAsync(customer);
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllWithDetailsAsync(pageNumber: 1, pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_WithStatusFilter_ReturnsFilteredOrders()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var pendingOrder = TestDataBuilder.CreateOrderEntity(customerId: customer.Id, orderStatus: OrderStatus.Pending);
        var completedOrder = TestDataBuilder.CreateOrderEntity(customerId: customer.Id, orderStatus: OrderStatus.Completed);

        await _context.Customers.AddAsync(customer);
        await _context.Orders.AddRangeAsync(pendingOrder, completedOrder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllWithDetailsAsync(pageNumber: 1, pageSize: 10, orderStatus: OrderStatus.Pending);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().OrderStatus.Should().Be(OrderStatus.Pending);
    }

    #endregion

    #region GetOrdersByCustomerIdAsync Tests

    [Fact]
    public async Task GetOrdersByCustomerIdAsync_WhenOrdersExist_ReturnsCustomerOrders()
    {
        // Arrange
        var customer1 = TestDataBuilder.CreateCustomerEntity();
        var customer2 = TestDataBuilder.CreateCustomerEntity();
        var order1 = TestDataBuilder.CreateOrderEntity(customerId: customer1.Id);
        var order2 = TestDataBuilder.CreateOrderEntity(customerId: customer1.Id);
        var order3 = TestDataBuilder.CreateOrderEntity(customerId: customer2.Id);

        await _context.Customers.AddRangeAsync(customer1, customer2);
        await _context.Orders.AddRangeAsync(order1, order2, order3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrdersByCustomerIdAsync(customer1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.All(o => o.CustomerId == customer1.Id).Should().BeTrue();
    }

    [Fact]
    public async Task GetOrdersByCustomerIdAsync_WhenNoOrders_ReturnsEmptyList()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var result = await _repository.GetOrdersByCustomerIdAsync(customerId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidOrder_SetsOrderDateAndUpdatedAt()
    {
        // Arrange
        var beforeAdd = DateTime.UtcNow;
        var customer = TestDataBuilder.CreateCustomerEntity();
        var order = TestDataBuilder.CreateOrderEntity(customerId: customer.Id);
        await _context.Customers.AddAsync(customer);

        // Act
        var result = await _repository.AddAsync(order);
        var afterAdd = DateTime.UtcNow;

        // Assert
        result.OrderDate.Should().BeOnOrAfter(beforeAdd);
        result.OrderDate.Should().BeOnOrBefore(afterAdd);
        result.UpdatedAt.Should().BeOnOrAfter(beforeAdd);
        result.UpdatedAt.Should().BeOnOrBefore(afterAdd);
    }

    [Fact]
    public async Task AddAsync_WithValidOrder_GeneratesNewGuid()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var order = TestDataBuilder.CreateOrderEntity(id: Guid.Empty, customerId: customer.Id);
        await _context.Customers.AddAsync(customer);

        // Act
        var result = await _repository.AddAsync(order);

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task AddAsync_WithValidOrder_SavesOrderToDatabase()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var order = TestDataBuilder.CreateOrderEntity(customerId: customer.Id);
        await _context.Customers.AddAsync(customer);

        // Act
        var result = await _repository.AddAsync(order);

        // Assert
        var savedOrder = await _context.Orders.FindAsync(result.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.OrderNumber.Should().Be(order.OrderNumber);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidOrder_UpdatesOrderStatus()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var order = TestDataBuilder.CreateOrderEntity(customerId: customer.Id, orderStatus: OrderStatus.Pending);
        await _context.Customers.AddAsync(customer);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        order.OrderStatus = OrderStatus.Completed;

        // Act
        var result = await _repository.UpdateAsync(order);

        // Assert
        result.OrderStatus.Should().Be(OrderStatus.Completed);
        var updatedOrder = await _context.Orders.FindAsync(order.Id);
        updatedOrder!.OrderStatus.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task UpdateAsync_WithValidOrder_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var order = TestDataBuilder.CreateOrderEntity(customerId: customer.Id);
        await _context.Customers.AddAsync(customer);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var beforeUpdate = DateTime.UtcNow;
        order.TotalAmount = 200m;

        // Act
        var result = await _repository.UpdateAsync(order);
        var afterUpdate = DateTime.UtcNow;

        // Assert
        result.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
        result.UpdatedAt.Should().BeOnOrBefore(afterUpdate);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenOrderExists_DeletesOrder()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var order = TestDataBuilder.CreateOrderEntity(customerId: customer.Id);
        await _context.Customers.AddAsync(customer);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(order.Id);

        // Assert
        result.Should().BeTrue();
        var deletedOrder = await _context.Orders.FindAsync(order.Id);
        deletedOrder.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenOrderNotFound_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}

