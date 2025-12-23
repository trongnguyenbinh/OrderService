using Domain;
using LegacyOrder.Tests.TestFixtures;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.Tests.UnitTests.Repositories;

public class CustomerRepositoryTests : IDisposable
{
    private readonly DataContext _context;
    private readonly CustomerRepository _repository;
    private readonly Mock<ILogger<CustomerRepository>> _mockLogger;

    public CustomerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mockLogger = LoggerFixture.CreateLogger<CustomerRepository>();
        _repository = new CustomerRepository(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ReturnsCustomer()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.FirstName.Should().Be(customer.FirstName);
        result.Email.Should().Be(customer.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerNotFound_ReturnsNull()
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
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        var trackedEntities = _context.ChangeTracker.Entries<CustomerEntity>().ToList();
        trackedEntities.Should().BeEmpty();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenCustomersExist_ReturnsAllCustomers()
    {
        // Arrange
        var customers = TestDataBuilder.CreateCustomerEntityList(5);
        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCustomers_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var customer1 = TestDataBuilder.CreateCustomerEntity(firstName: "Customer1", createdAt: DateTime.UtcNow.AddDays(-2));
        var customer2 = TestDataBuilder.CreateCustomerEntity(firstName: "Customer2", createdAt: DateTime.UtcNow.AddDays(-1));
        var customer3 = TestDataBuilder.CreateCustomerEntity(firstName: "Customer3", createdAt: DateTime.UtcNow);

        await _context.Customers.AddRangeAsync(customer1, customer2, customer3);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].FirstName.Should().Be("Customer3");
        result[1].FirstName.Should().Be("Customer2");
        result[2].FirstName.Should().Be("Customer1");
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidCustomer_SavesAndReturnsCustomer()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();

        // Act
        var result = await _repository.AddAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        var savedCustomer = await _context.Customers.FindAsync(result.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Email.Should().Be(customer.Email);
    }

    [Fact]
    public async Task AddAsync_SetsCreatedAtAndUpdatedAt()
    {
        // Arrange
        var beforeAdd = DateTime.UtcNow;
        var customer = TestDataBuilder.CreateCustomerEntity();

        // Act
        var result = await _repository.AddAsync(customer);
        var afterAdd = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeOnOrAfter(beforeAdd);
        result.CreatedAt.Should().BeOnOrBefore(afterAdd);
        result.UpdatedAt.Should().BeOnOrAfter(beforeAdd);
        result.UpdatedAt.Should().BeOnOrBefore(afterAdd);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidCustomer_UpdatesAndReturnsCustomer()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity(firstName: "John", email: "john@example.com");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var updatedCustomer = TestDataBuilder.CreateCustomerEntity(
            id: customer.Id,
            firstName: "Jane",
            email: "jane@example.com"
        );

        // Act
        var result = await _repository.UpdateAsync(updatedCustomer);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task UpdateAsync_PreservesCreatedAt()
    {
        // Arrange
        var originalCreatedAt = DateTime.UtcNow.AddDays(-5);
        var customer = TestDataBuilder.CreateCustomerEntity(createdAt: originalCreatedAt);
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var updatedCustomer = TestDataBuilder.CreateCustomerEntity(id: customer.Id, firstName: "Updated");

        // Act
        var result = await _repository.UpdateAsync(updatedCustomer);

        // Assert
        result.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentCustomer = TestDataBuilder.CreateCustomerEntity(id: Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.UpdateAsync(nonExistentCustomer));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenCustomerExists_ReturnsTrue()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(customer.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerNotFound_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesCustomer()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(customer.Id);

        // Assert
        var deletedCustomer = await _context.Customers.FindAsync(customer.Id);
        deletedCustomer.Should().NotBeNull();
        deletedCustomer!.IsActive.Should().BeFalse();
    }

    #endregion

    #region GetByEmailAsync Tests

    [Fact]
    public async Task GetByEmailAsync_WhenEmailExists_ReturnsCustomer()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity(email: "test@example.com");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenEmailNotFound_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithExactEmail_ReturnsCustomer()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity(email: "Test@Example.com");
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("Test@Example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("Test@Example.com");
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_WithNoFilters_ReturnsAllActiveCustomers()
    {
        // Arrange
        var customer1 = TestDataBuilder.CreateCustomerEntity(isActive: true);
        var customer2 = TestDataBuilder.CreateCustomerEntity(isActive: true);
        var customer3 = TestDataBuilder.CreateCustomerEntity(isActive: false);
        await _context.Customers.AddRangeAsync(customer1, customer2, customer3);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateCustomerSearchRequest();

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_FiltersByFirstName()
    {
        // Arrange
        var customer1 = TestDataBuilder.CreateCustomerEntity(firstName: "John");
        var customer2 = TestDataBuilder.CreateCustomerEntity(firstName: "Jane");
        await _context.Customers.AddRangeAsync(customer1, customer2);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateCustomerSearchRequest(firstName: "John");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().FirstName.Should().Contain("John");
    }

    [Fact]
    public async Task SearchAsync_FiltersByLastName()
    {
        // Arrange
        var customer1 = TestDataBuilder.CreateCustomerEntity(lastName: "Doe");
        var customer2 = TestDataBuilder.CreateCustomerEntity(lastName: "Smith");
        await _context.Customers.AddRangeAsync(customer1, customer2);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateCustomerSearchRequest(lastName: "Doe");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().LastName.Should().Contain("Doe");
    }

    [Fact]
    public async Task SearchAsync_FiltersByEmail()
    {
        // Arrange
        var customer1 = TestDataBuilder.CreateCustomerEntity(email: "john@example.com");
        var customer2 = TestDataBuilder.CreateCustomerEntity(email: "jane@example.com");
        await _context.Customers.AddRangeAsync(customer1, customer2);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateCustomerSearchRequest(email: "john");

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Email.Should().Contain("john");
    }

    [Fact]
    public async Task SearchAsync_FiltersByCustomerType()
    {
        // Arrange
        var customer1 = TestDataBuilder.CreateCustomerEntity(customerType: CustomerType.Premium);
        var customer2 = TestDataBuilder.CreateCustomerEntity(customerType: CustomerType.Regular);
        await _context.Customers.AddRangeAsync(customer1, customer2);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateCustomerSearchRequest(customerType: CustomerType.Premium);

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().CustomerType.Should().Be(CustomerType.Premium);
    }

    [Fact]
    public async Task SearchAsync_SupportsPagination()
    {
        // Arrange
        var customers = TestDataBuilder.CreateCustomerEntityList(15);
        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();

        var request = TestDataBuilder.CreateCustomerSearchRequest(pageNumber: 2, pageSize: 5);

        // Act
        var result = await _repository.SearchAsync(request);

        // Assert
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
        result.Items.Should().HaveCount(5);
    }

    #endregion
}

