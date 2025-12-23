using Domain;
using LegacyOrder.Tests.TestFixtures;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.Tests.IntegrationTests;

public class CustomerFlowIntegrationTests : IDisposable
{
    private readonly DataContext _context;
    private readonly CustomerRepository _repository;
    private readonly CustomerService _service;
    private readonly IMapper _mapper;

    public CustomerFlowIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mapper = AutoMapperFixture.CreateMapper();
        
        var repositoryLogger = LoggerFixture.CreateNullLogger<CustomerRepository>();
        _repository = new CustomerRepository(_context, repositoryLogger);
        
        var serviceLogger = LoggerFixture.CreateNullLogger<CustomerService>();
        _service = new CustomerService(_repository, serviceLogger, _mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Create-Read-Update-Delete Flow Tests

    [Fact]
    public async Task CustomerFlow_CreateAndRetrieve_Success()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCustomerRequest(
            firstName: "John",
            lastName: "Doe",
            email: "john.doe@example.com",
            phoneNumber: "555-123-4567"
        );

        // Act - Create
        var createdCustomer = await _service.CreateAsync(createRequest);

        // Act - Retrieve
        var retrievedCustomer = await _service.GetByIdAsync(createdCustomer.Id);

        // Assert
        retrievedCustomer.Should().NotBeNull();
        retrievedCustomer!.Id.Should().Be(createdCustomer.Id);
        retrievedCustomer.FirstName.Should().Be(createRequest.FirstName);
        retrievedCustomer.LastName.Should().Be(createRequest.LastName);
        retrievedCustomer.Email.Should().Be(createRequest.Email);
        retrievedCustomer.PhoneNumber.Should().Be(createRequest.PhoneNumber);
    }

    [Fact]
    public async Task CustomerFlow_CreateUpdateAndRetrieve_Success()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCustomerRequest(
            firstName: "John",
            lastName: "Doe",
            email: "john.doe@example.com"
        );

        var updateRequest = TestDataBuilder.CreateUpdateCustomerRequest(
            firstName: "Jane",
            lastName: "Smith",
            email: "jane.smith@example.com"
        );

        // Act - Create
        var createdCustomer = await _service.CreateAsync(createRequest);

        // Act - Update
        var updatedCustomer = await _service.UpdateAsync(createdCustomer.Id, updateRequest);

        // Act - Retrieve
        var retrievedCustomer = await _service.GetByIdAsync(createdCustomer.Id);

        // Assert
        retrievedCustomer.Should().NotBeNull();
        retrievedCustomer!.FirstName.Should().Be(updateRequest.FirstName);
        retrievedCustomer.LastName.Should().Be(updateRequest.LastName);
        retrievedCustomer.Email.Should().Be(updateRequest.Email);
    }

    [Fact]
    public async Task CustomerFlow_CreateDeleteAndRetrieve_CustomerNotActive()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCustomerRequest(
            firstName: "John",
            lastName: "Doe",
            email: "john.doe@example.com"
        );

        // Act - Create
        var createdCustomer = await _service.CreateAsync(createRequest);

        // Act - Delete (soft delete)
        var deleteResult = await _service.DeleteAsync(createdCustomer.Id);

        // Act - Retrieve from repository directly to check soft delete
        var deletedEntity = await _repository.GetByIdAsync(createdCustomer.Id);

        // Assert
        deleteResult.Should().BeTrue();
        deletedEntity.Should().NotBeNull();
        deletedEntity!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CustomerFlow_CreateMultipleAndSearch_ReturnsFilteredResults()
    {
        // Arrange
        var customers = new[]
        {
            TestDataBuilder.CreateCustomerRequest(firstName: "John", lastName: "Doe", email: "john@example.com", customerType: CustomerType.Regular),
            TestDataBuilder.CreateCustomerRequest(firstName: "Jane", lastName: "Smith", email: "jane@example.com", customerType: CustomerType.Premium),
            TestDataBuilder.CreateCustomerRequest(firstName: "Bob", lastName: "Johnson", email: "bob@example.com", customerType: CustomerType.Regular),
            TestDataBuilder.CreateCustomerRequest(firstName: "Alice", lastName: "Williams", email: "alice@example.com", customerType: CustomerType.VIP)
        };

        foreach (var customer in customers)
        {
            await _service.CreateAsync(customer);
        }

        var searchRequest = TestDataBuilder.CreateCustomerSearchRequest(
            firstName: "John"
        );

        // Act
        var searchResult = await _service.SearchAsync(searchRequest);

        // Assert
        searchResult.Should().NotBeNull();
        searchResult.Items.Should().HaveCount(1);
        searchResult.Items.First().FirstName.Should().Contain("John");
        searchResult.TotalCount.Should().Be(1);
    }

    #endregion

    #region Pagination and Sorting Tests

    [Fact]
    public async Task CustomerFlow_SearchWithPagination_ReturnsCorrectPage()
    {
        // Arrange - Create 15 customers
        for (int i = 1; i <= 15; i++)
        {
            var request = TestDataBuilder.CreateCustomerRequest(
                firstName: $"Customer{i}",
                lastName: $"Last{i}",
                email: $"customer{i}@example.com"
            );
            await _service.CreateAsync(request);
        }

        var searchRequest = TestDataBuilder.CreateCustomerSearchRequest(
            pageNumber: 2,
            pageSize: 5
        );

        // Act
        var result = await _service.SearchAsync(searchRequest);

        // Assert
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task CustomerFlow_SearchByCustomerType_ReturnsSortedResults()
    {
        // Arrange
        var customers = new[]
        {
            TestDataBuilder.CreateCustomerRequest(firstName: "Customer A", lastName: "Last A", email: "a@example.com", customerType: CustomerType.Regular),
            TestDataBuilder.CreateCustomerRequest(firstName: "Customer B", lastName: "Last B", email: "b@example.com", customerType: CustomerType.Premium),
            TestDataBuilder.CreateCustomerRequest(firstName: "Customer C", lastName: "Last C", email: "c@example.com", customerType: CustomerType.VIP)
        };

        foreach (var customer in customers)
        {
            await _service.CreateAsync(customer);
        }

        var searchRequest = TestDataBuilder.CreateCustomerSearchRequest(
            customerType: CustomerType.Premium
        );

        // Act
        var result = await _service.SearchAsync(searchRequest);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().CustomerType.Should().Be(CustomerType.Premium);
    }

    #endregion

    #region Business Rule Validation Tests

    [Fact]
    public async Task CustomerFlow_CreateWithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request1 = TestDataBuilder.CreateCustomerRequest(email: "duplicate@example.com");
        var request2 = TestDataBuilder.CreateCustomerRequest(email: "duplicate@example.com");

        await _service.CreateAsync(request1);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request2);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*duplicate@example.com*already exists*");
    }

    [Fact]
    public async Task CustomerFlow_UpdateWithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var customer1 = await _service.CreateAsync(
            TestDataBuilder.CreateCustomerRequest(email: "customer1@example.com")
        );
        var customer2 = await _service.CreateAsync(
            TestDataBuilder.CreateCustomerRequest(email: "customer2@example.com")
        );

        var updateRequest = TestDataBuilder.CreateUpdateCustomerRequest(email: "customer1@example.com");

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(customer2.Id, updateRequest);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*customer1@example.com*already used*");
    }

    [Fact]
    public async Task CustomerFlow_CreateWithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest(email: "invalid-email");

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CustomerFlow_CreateWithEmptyFirstName_ThrowsArgumentException()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest(firstName: "");

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region GetByEmail Tests

    [Fact]
    public async Task CustomerFlow_GetByEmail_ReturnsCorrectCustomer()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCustomerRequest(
            firstName: "John",
            email: "john@example.com"
        );

        var createdCustomer = await _service.CreateAsync(createRequest);

        // Act
        var retrievedCustomer = await _service.GetByEmailAsync("john@example.com");

        // Assert
        retrievedCustomer.Should().NotBeNull();
        retrievedCustomer!.Id.Should().Be(createdCustomer.Id);
        retrievedCustomer.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task CustomerFlow_GetByNonExistentEmail_ReturnsNull()
    {
        // Act
        var result = await _service.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task CustomerFlow_GetAll_ReturnsAllActiveCustomers()
    {
        // Arrange
        var customer1 = await _service.CreateAsync(
            TestDataBuilder.CreateCustomerRequest(email: "customer1@example.com")
        );
        var customer2 = await _service.CreateAsync(
            TestDataBuilder.CreateCustomerRequest(email: "customer2@example.com")
        );

        // Act
        var allCustomers = await _service.GetAllAsync();

        // Assert
        allCustomers.Should().HaveCount(2);
        allCustomers.Should().Contain(c => c.Id == customer1.Id);
        allCustomers.Should().Contain(c => c.Id == customer2.Id);
    }

    [Fact]
    public async Task CustomerFlow_SearchExcludesDeletedCustomers()
    {
        // Arrange
        var customer1 = await _service.CreateAsync(
            TestDataBuilder.CreateCustomerRequest(email: "customer1@example.com")
        );
        var customer2 = await _service.CreateAsync(
            TestDataBuilder.CreateCustomerRequest(email: "customer2@example.com")
        );

        // Act - Delete customer1
        await _service.DeleteAsync(customer1.Id);

        // Act - Search (which filters by IsActive=true)
        var searchRequest = TestDataBuilder.CreateCustomerSearchRequest();
        var searchResult = await _service.SearchAsync(searchRequest);

        // Assert
        searchResult.Items.Should().HaveCount(1);
        searchResult.Items.Should().NotContain(c => c.Id == customer1.Id);
        searchResult.Items.Should().Contain(c => c.Id == customer2.Id);
    }

    #endregion
}

