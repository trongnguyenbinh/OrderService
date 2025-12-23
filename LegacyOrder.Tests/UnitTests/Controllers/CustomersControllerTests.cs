using LegacyOrder.Controllers;
using LegacyOrder.Tests.TestFixtures;
using Microsoft.AspNetCore.Mvc;

namespace LegacyOrder.Tests.UnitTests.Controllers;

public class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _mockService;
    private readonly Mock<ILogger<CustomersController>> _mockLogger;
    private readonly CustomersController _controller;

    public CustomersControllerTests()
    {
        _mockService = new Mock<ICustomerService>();
        _mockLogger = LoggerFixture.CreateLogger<CustomersController>();
        _controller = new CustomersController(_mockService.Object, _mockLogger.Object);
    }

    #region SearchCustomers Tests

    [Fact]
    public async Task SearchCustomers_WithValidRequest_ReturnsOkWithPagedResults()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerSearchRequest();
        var pagedResult = new PagedResult<CustomerDto>
        {
            Items = new List<CustomerDto> { TestDataBuilder.CreateCustomerDto() },
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            TotalPages = 1
        };

        _mockService
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.SearchCustomers(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedData = okResult.Value.Should().BeOfType<PagedResult<CustomerDto>>().Subject;
        returnedData.Items.Should().HaveCount(1);
        returnedData.PageNumber.Should().Be(1);
        returnedData.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchCustomers_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerSearchRequest(
            firstName: "John",
            pageNumber: 2,
            pageSize: 20
        );
        var pagedResult = new PagedResult<CustomerDto>
        {
            Items = new List<CustomerDto>(),
            PageNumber = 2,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };

        _mockService
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        await _controller.SearchCustomers(request, CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.SearchAsync(
                It.Is<CustomerSearchRequest>(r =>
                    r.FirstName == "John" &&
                    r.PageNumber == 2 &&
                    r.PageSize == 20),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenCustomerExists_ReturnsOkWithCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerDto = TestDataBuilder.CreateCustomerDto(id: customerId);

        _mockService
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerDto);

        // Act
        var result = await _controller.GetById(customerId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCustomer = okResult.Value.Should().BeOfType<CustomerDto>().Subject;
        returnedCustomer.Id.Should().Be(customerId);
    }

    [Fact]
    public async Task GetById_WhenCustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _mockService
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetById(customerId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenCustomerNotFound_ReturnsErrorMessage()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _mockService
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetById(customerId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().NotBeNull();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest();
        var createdCustomer = TestDataBuilder.CreateCustomerDto(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email
        );

        _mockService
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCustomer);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(CustomersController.GetById));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(createdCustomer.Id);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedCustomer()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest();
        var createdCustomer = TestDataBuilder.CreateCustomerDto(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email
        );

        _mockService
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCustomer);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedCustomer = createdResult.Value.Should().BeOfType<CustomerDto>().Subject;
        returnedCustomer.FirstName.Should().Be(request.FirstName);
        returnedCustomer.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Create_CallsServiceWithCorrectRequest()
    {
        // Arrange
        var request = TestDataBuilder.CreateCustomerRequest(
            firstName: "John",
            lastName: "Doe",
            email: "john.doe@example.com"
        );
        var createdCustomer = TestDataBuilder.CreateCustomerDto();

        _mockService
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCustomer);

        // Act
        await _controller.Create(request, CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.CreateAsync(
                It.Is<CreateCustomerRequest>(r =>
                    r.FirstName == "John" &&
                    r.LastName == "Doe" &&
                    r.Email == "john.doe@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidRequest_ReturnsOkWithUpdatedCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateCustomerRequest();
        var updatedCustomer = TestDataBuilder.CreateCustomerDto(
            id: customerId,
            firstName: request.FirstName,
            lastName: request.LastName
        );

        _mockService
            .Setup(x => x.UpdateAsync(customerId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.Update(customerId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCustomer = okResult.Value.Should().BeOfType<CustomerDto>().Subject;
        returnedCustomer.Id.Should().Be(customerId);
        returnedCustomer.FirstName.Should().Be(request.FirstName);
    }

    [Fact]
    public async Task Update_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateCustomerRequest(
            firstName: "Jane",
            lastName: "Smith"
        );
        var updatedCustomer = TestDataBuilder.CreateCustomerDto();

        _mockService
            .Setup(x => x.UpdateAsync(customerId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCustomer);

        // Act
        await _controller.Update(customerId, request, CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.UpdateAsync(
                customerId,
                It.Is<UpdateCustomerRequest>(r =>
                    r.FirstName == "Jane" &&
                    r.LastName == "Smith"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenCustomerExists_ReturnsNoContent()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _mockService
            .Setup(x => x.DeleteAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(customerId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenCustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _mockService
            .Setup(x => x.DeleteAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(customerId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsServiceWithCorrectId()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _mockService
            .Setup(x => x.DeleteAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _controller.Delete(customerId, CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.DeleteAsync(customerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetCustomerOrders Tests

    [Fact]
    public async Task GetCustomerOrders_WhenCustomerExists_ReturnsOkWithOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerDto = TestDataBuilder.CreateCustomerDto(id: customerId);
        var orders = new List<OrderDto>
        {
            new OrderDto { Id = Guid.NewGuid(), OrderNumber = "ORD-001", CustomerId = customerId },
            new OrderDto { Id = Guid.NewGuid(), OrderNumber = "ORD-002", CustomerId = customerId }
        };

        _mockService
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerDto);

        _mockService
            .Setup(x => x.GetCustomerOrdersAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetCustomerOrders(customerId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrders = okResult.Value.Should().BeOfType<List<OrderDto>>().Subject;
        returnedOrders.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCustomerOrders_WhenCustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _mockService
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetCustomerOrders(customerId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetCustomerOrders_WhenCustomerExistsButNoOrders_ReturnsOkWithEmptyList()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerDto = TestDataBuilder.CreateCustomerDto(id: customerId);

        _mockService
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerDto);

        _mockService
            .Setup(x => x.GetCustomerOrdersAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderDto>());

        // Act
        var result = await _controller.GetCustomerOrders(customerId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrders = okResult.Value.Should().BeOfType<List<OrderDto>>().Subject;
        returnedOrders.Should().BeEmpty();
    }

    #endregion
}

