using Domain;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.Tests.UnitTests.Repositories;

public class CustomerRepositoryAdditionalTests
{
    private readonly DataContext _context;
    private readonly Mock<ILogger<CustomerRepository>> _mockLogger;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryAdditionalTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mockLogger = new Mock<ILogger<CustomerRepository>>();
        _repository = new CustomerRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ReturnsCustomer()
    {
        // Arrange
        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            IsActive = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("john@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_WithFirstNameFilter_ReturnsMatchingCustomers()
    {
        // Arrange
        var customer1 = new CustomerEntity { FirstName = "John", LastName = "Doe", Email = "john@example.com", IsActive = true };
        var customer2 = new CustomerEntity { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", IsActive = true };
        _context.Customers.AddRange(customer1, customer2);
        await _context.SaveChangesAsync();

        var request = new CustomerSearchRequest { FirstName = "John", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().FirstName.Should().Be("John");
    }

    [Fact]
    public async Task SearchAsync_WithLastNameFilter_ReturnsMatchingCustomers()
    {
        // Arrange
        var customer1 = new CustomerEntity { FirstName = "John", LastName = "Doe", Email = "john@example.com", IsActive = true };
        var customer2 = new CustomerEntity { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", IsActive = true };
        _context.Customers.AddRange(customer1, customer2);
        await _context.SaveChangesAsync();

        var request = new CustomerSearchRequest { LastName = "Smith", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task SearchAsync_WithCustomerTypeFilter_ReturnsMatchingCustomers()
    {
        // Arrange
        var customer1 = new CustomerEntity { FirstName = "John", LastName = "Doe", Email = "john@example.com", CustomerType = CustomerType.Premium, IsActive = true };
        var customer2 = new CustomerEntity { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", CustomerType = CustomerType.Regular, IsActive = true };
        _context.Customers.AddRange(customer1, customer2);
        await _context.SaveChangesAsync();

        var request = new CustomerSearchRequest { CustomerType = CustomerType.Premium, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().CustomerType.Should().Be(CustomerType.Premium);
    }

    [Fact]
    public async Task SearchAsync_WithSortByFirstName_ReturnsSortedResults()
    {
        // Arrange
        var customer1 = new CustomerEntity { FirstName = "Zoe", LastName = "Doe", Email = "zoe@example.com", IsActive = true };
        var customer2 = new CustomerEntity { FirstName = "Alice", LastName = "Smith", Email = "alice@example.com", IsActive = true };
        _context.Customers.AddRange(customer1, customer2);
        await _context.SaveChangesAsync();

        var request = new CustomerSearchRequest { SortBy = "firstname", SortDirection = "asc", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.First().FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_WithExistingCustomer_ReturnsOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new CustomerEntity { Id = customerId, FirstName = "John", LastName = "Doe", Email = "john@example.com", IsActive = true };
        var order = new OrderEntity { CustomerId = customerId, OrderNumber = "ORD-001", OrderStatus = OrderStatus.Pending };
        
        _context.Customers.Add(customer);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCustomerOrdersAsync(customerId);

        // Assert
        result.Should().HaveCount(1);
        result.First().OrderNumber.Should().Be("ORD-001");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentCustomer_ThrowsKeyNotFoundException()
    {
        // Arrange
        var customer = new CustomerEntity { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@example.com" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(customer));
    }
}

