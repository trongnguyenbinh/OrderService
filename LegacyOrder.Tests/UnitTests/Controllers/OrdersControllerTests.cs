using LegacyOrder.Controllers;
using LegacyOrder.Tests.TestFixtures;
using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Common.Exceptions;

namespace LegacyOrder.Tests.UnitTests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _mockService;
    private readonly Mock<ILogger<OrdersController>> _mockLogger;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mockService = new Mock<IOrderService>();
        _mockLogger = LoggerFixture.CreateLogger<OrdersController>();
        _controller = new OrdersController(_mockService.Object, _mockLogger.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidParameters_ReturnsOkWithPagedResults()
    {
        // Arrange
        var pagedResult = new PagedResult<OrderDto>
        {
            Items = new List<OrderDto> { TestDataBuilder.CreateOrderDto() },
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            TotalPages = 1
        };

        _mockService
            .Setup(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(1, 10, null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedData = okResult.Value.Should().BeOfType<PagedResult<OrderDto>>().Subject;
        returnedData.Items.Should().HaveCount(1);
        returnedData.PageNumber.Should().Be(1);
        returnedData.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsFilteredOrders()
    {
        // Arrange
        var pagedResult = new PagedResult<OrderDto>
        {
            Items = new List<OrderDto> { TestDataBuilder.CreateOrderDto(orderStatus: OrderStatus.Pending) },
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            TotalPages = 1
        };

        _mockService
            .Setup(x => x.GetAllAsync(1, 10, OrderStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(1, 10, OrderStatus.Pending, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedData = okResult.Value.Should().BeOfType<PagedResult<OrderDto>>().Subject;
        returnedData.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAll_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAll(0, 10, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAll_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAll(1, 101, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenOrderExists_ReturnsOkWithOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = TestDataBuilder.CreateOrderDto(id: orderId);

        _mockService
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.GetById(orderId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrder = okResult.Value.Should().BeOfType<OrderDto>().Subject;
        returnedOrder.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task GetById_WhenOrderNotFound_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mockService
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.GetById(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtActionWithOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 2)
            }
        );
        var createdOrder = TestDataBuilder.CreateOrderDto(customerId: customerId);

        _mockService
            .Setup(x => x.CreateOrderAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(OrdersController.GetById));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(createdOrder.Id);
        var returnedOrder = createdResult.Value.Should().BeOfType<OrderDto>().Subject;
        returnedOrder.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task Create_WhenCustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 2)
            }
        );

        _mockService
            .Setup(x => x.CreateOrderAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Customer", customerId));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WhenInsufficientStock_ReturnsBadRequest()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 100)
            }
        );

        _mockService
            .Setup(x => x.CreateOrderAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InsufficientStockException("Product", 5, 100));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Complete Tests

    [Fact]
    public async Task Complete_WithPendingOrder_ReturnsOkWithCompletedOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var completedOrder = TestDataBuilder.CreateOrderDto(
            id: orderId,
            orderStatus: OrderStatus.Completed
        );

        _mockService
            .Setup(x => x.CompleteOrderAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedOrder);

        // Act
        var result = await _controller.Complete(orderId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrder = okResult.Value.Should().BeOfType<OrderDto>().Subject;
        returnedOrder.OrderStatus.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Complete_WhenOrderNotFound_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mockService
            .Setup(x => x.CompleteOrderAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Order", orderId));

        // Act
        var result = await _controller.Complete(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Complete_WhenOrderAlreadyCompleted_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mockService
            .Setup(x => x.CompleteOrderAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOrderStatusException("Order is already completed"));

        // Act
        var result = await _controller.Complete(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public async Task Cancel_WithPendingOrder_ReturnsOkWithCancelledOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var cancelledOrder = TestDataBuilder.CreateOrderDto(
            id: orderId,
            orderStatus: OrderStatus.Cancelled
        );

        _mockService
            .Setup(x => x.CancelOrderAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelledOrder);

        // Act
        var result = await _controller.Cancel(orderId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrder = okResult.Value.Should().BeOfType<OrderDto>().Subject;
        returnedOrder.OrderStatus.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_WhenOrderNotFound_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mockService
            .Setup(x => x.CancelOrderAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Order", orderId));

        // Act
        var result = await _controller.Cancel(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Cancel_WhenOrderNotPending_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mockService
            .Setup(x => x.CancelOrderAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOrderStatusException("Completed", "cancel"));

        // Act
        var result = await _controller.Cancel(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}

