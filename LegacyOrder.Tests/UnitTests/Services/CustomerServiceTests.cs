using LegacyOrder.Tests.TestFixtures;

namespace LegacyOrder.Tests.UnitTests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _mockRepository = new Mock<ICustomerRepository>();
        _mockLogger = LoggerFixture.CreateLogger<CustomerService>();
        _mapper = AutoMapperFixture.CreateMapper();
        _service = new CustomerService(_mockRepository.Object, _mockLogger.Object, _mapper);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ReturnsCustomerDto()
    {
        // Arrange
        var customerEntity = TestDataBuilder.CreateCustomerEntity();
        _mockRepository
            .Setup(x => x.GetByIdAsync(customerEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerEntity);

        // Act
        var result = await _service.GetByIdAsync(customerEntity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerEntity.Id);
        result.FirstName.Should().Be(customerEntity.FirstName);
        result.Email.Should().Be(customerEntity.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockRepository
            .Setup(x => x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        // Act
        var result = await _service.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenCustomersExist_ReturnsCustomerDtoList()
    {
        // Arrange
        var customerEntities = TestDataBuilder.CreateCustomerEntityList(5);
        _mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerEntities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Should().AllBeOfType<CustomerDto>();
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCustomers_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustomerEntity>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedCustomerDto()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest();
        var customerEntity = TestDataBuilder.CreateCustomerEntity(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email
        );

        _mockRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerEntity);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.Email.Should().Be(request.Email);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithInvalidFirstName_ThrowsArgumentException(string invalidFirstName)
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest(firstName: invalidFirstName);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithInvalidLastName_ThrowsArgumentException(string invalidLastName)
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest(lastName: invalidLastName);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithEmptyEmail_ThrowsArgumentException(string invalidEmail)
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest(email: invalidEmail);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("invalid@")]
    public async Task CreateAsync_WithInvalidEmailFormat_ThrowsArgumentException(string invalidEmail)
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest(email: invalidEmail);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest();
        var existingCustomer = TestDataBuilder.CreateCustomerEntity(email: request.Email);

        _mockRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ReturnsUpdatedCustomerDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var existingCustomer = TestDataBuilder.CreateCustomerEntity(id: customerId, firstName: "John");
        var request = TestDataBuilder.CreateUpdateCustomerRequest(firstName: "Jane");
        var updatedEntity = TestDataBuilder.CreateCustomerEntity(id: customerId, firstName: "Jane");

        _mockRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _mockRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEntity);

        // Act
        var result = await _service.UpdateAsync(customerId, request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateCustomerRequest();

        _mockRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(customerId, request));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var existingCustomer = TestDataBuilder.CreateCustomerEntity(id: customerId, email: "john@example.com");
        var anotherCustomer = TestDataBuilder.CreateCustomerEntity(email: "jane@example.com");
        var request = TestDataBuilder.CreateUpdateCustomerRequest(email: "jane@example.com");

        _mockRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _mockRepository
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(anotherCustomer);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(customerId, request));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenCustomerExists_ReturnsTrue()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _mockRepository
            .Setup(x => x.DeleteAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(customerId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerNotFound_ReturnsFalse()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _mockRepository
            .Setup(x => x.DeleteAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(customerId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetByEmailAsync Tests

    [Fact]
    public async Task GetByEmailAsync_WhenEmailExists_ReturnsCustomerDto()
    {
        // Arrange
        var email = "john@example.com";
        var customerEntity = TestDataBuilder.CreateCustomerEntity(email: email);
        _mockRepository
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerEntity);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenEmailNotFound_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _mockRepository
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_WithValidRequest_ReturnsPagedResult()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerSearchRequest();
        var customers = TestDataBuilder.CreateCustomerEntityList(5);
        var pagedResult = new PagedResult<CustomerEntity>
        {
            Items = customers,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 5,
            TotalPages = 1
        };

        _mockRepository
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(1);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task SearchAsync_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerSearchRequest(firstName: "John", pageNumber: 2, pageSize: 20);
        var pagedResult = new PagedResult<CustomerEntity>
        {
            Items = new List<CustomerEntity>(),
            PageNumber = 2,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };

        _mockRepository
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        await _service.SearchAsync(request);

        // Assert
        _mockRepository.Verify(x => x.SearchAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetCustomerOrdersAsync Tests

    [Fact]
    public async Task GetCustomerOrdersAsync_WhenOrdersExist_ReturnsOrderDtoList()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orders = new List<OrderEntity>
        {
            new OrderEntity { Id = Guid.NewGuid(), OrderNumber = "ORD-001", CustomerId = customerId },
            new OrderEntity { Id = Guid.NewGuid(), OrderNumber = "ORD-002", CustomerId = customerId }
        };

        _mockRepository
            .Setup(x => x.GetCustomerOrdersAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _service.GetCustomerOrdersAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_WhenNoOrders_ReturnsEmptyList()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _mockRepository
            .Setup(x => x.GetCustomerOrdersAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderEntity>());

        // Act
        var result = await _service.GetCustomerOrdersAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion
}

