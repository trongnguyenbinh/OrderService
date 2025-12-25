namespace LegacyOrder.Tests.UnitTests.Services;

public class CustomerServiceAdditionalTests
{
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CustomerService _service;

    public CustomerServiceAdditionalTests()
    {
        _mockRepository = new Mock<ICustomerRepository>();
        _mockLogger = new Mock<ILogger<CustomerService>>();
        _mockMapper = new Mock<IMapper>();
        _service = new CustomerService(_mockRepository.Object, _mockLogger.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ReturnsCustomer()
    {
        // Arrange
        var customer = new CustomerEntity { Id = Guid.NewGuid(), Email = "test@example.com", FirstName = "John" };
        var dto = new CustomerDto { Id = customer.Id, Email = customer.Email };

        _mockRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _mockMapper.Setup(m => m.Map<CustomerDto>(customer)).Returns(dto);

        // Act
        var result = await _service.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByEmailAsync("nonexistent@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        // Act
        var result = await _service.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateCustomerRequest { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var existingCustomer = new CustomerEntity { Email = "john@example.com" };

        _mockRepository.Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_WithDifferentEmailThatExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var existingCustomer = new CustomerEntity { Id = customerId, Email = "old@example.com", FirstName = "John", LastName = "Doe" };
        var request = new UpdateCustomerRequest { FirstName = "John", LastName = "Doe", Email = "new@example.com" };
        var otherCustomer = new CustomerEntity { Id = Guid.NewGuid(), Email = "new@example.com" };

        _mockRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);
        _mockRepository.Setup(r => r.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherCustomer);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(customerId, request));
    }

    [Fact]
    public async Task SearchAsync_WithInvalidPageNumber_ThrowsArgumentException()
    {
        // Arrange
        var request = new CustomerSearchRequest { PageNumber = 0, PageSize = 10 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SearchAsync(request));
    }

    [Fact]
    public async Task SearchAsync_WithInvalidPageSize_ThrowsArgumentException()
    {
        // Arrange
        var request = new CustomerSearchRequest { PageNumber = 1, PageSize = 0 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SearchAsync(request));
    }

    [Fact]
    public async Task SearchAsync_WithPageSizeExceedingMax_ThrowsArgumentException()
    {
        // Arrange
        var request = new CustomerSearchRequest { PageNumber = 1, PageSize = 101 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SearchAsync(request));
    }

    [Fact]
    public async Task SearchAsync_WithInvalidSortDirection_ThrowsArgumentException()
    {
        // Arrange
        var request = new CustomerSearchRequest { PageNumber = 1, PageSize = 10, SortDirection = "invalid" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SearchAsync(request));
    }

    [Fact]
    public async Task SearchAsync_WithInvalidSortField_ThrowsArgumentException()
    {
        // Arrange
        var request = new CustomerSearchRequest { PageNumber = 1, PageSize = 10, SortBy = "invalidfield" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SearchAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest { FirstName = "John", LastName = "Doe", Email = "invalid-email" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidPhoneNumber_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest 
        { 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john@example.com",
            PhoneNumber = "123" // Too short
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_WithValidCustomerId_ReturnsOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orders = new List<OrderEntity> { new OrderEntity { CustomerId = customerId, OrderNumber = "ORD-001" } };
        var orderDtos = new List<OrderDto> { new OrderDto { OrderNumber = "ORD-001" } };

        _mockRepository.Setup(r => r.GetCustomerOrdersAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);
        _mockMapper.Setup(m => m.Map<IEnumerable<OrderDto>>(orders)).Returns(orderDtos);

        // Act
        var result = await _service.GetCustomerOrdersAsync(customerId);

        // Assert
        result.Should().HaveCount(1);
    }
}

