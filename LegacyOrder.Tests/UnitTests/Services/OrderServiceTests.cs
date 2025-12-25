using LegacyOrder.Tests.TestFixtures;
using Model.Enums;
using Common.Exceptions;

namespace LegacyOrder.Tests.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IPricingService> _mockPricingService;
    private readonly Mock<IInventoryService> _mockInventoryService;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        _mockPricingService = new Mock<IPricingService>();
        _mockInventoryService = new Mock<IInventoryService>();
        _mockLogger = LoggerFixture.CreateLogger<OrderService>();
        _mapper = AutoMapperFixture.CreateMapper();
        
        _service = new OrderService(
            _mockOrderRepository.Object,
            _mockCustomerRepository.Object,
            _mockProductRepository.Object,
            _mockPricingService.Object,
            _mockInventoryService.Object,
            _mockLogger.Object,
            _mapper
        );
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ReturnsOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var orderEntity = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            customerId: customerId,
            orderNumber: "ORD-001"
        );
        
        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderEntity);

        // Act
        var result = await _service.GetByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.OrderNumber.Should().Be("ORD-001");
        result.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderNotFound_ReturnsNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        // Act
        var result = await _service.GetByIdAsync(orderId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByOrderNumberAsync Tests

    [Fact]
    public async Task GetByOrderNumberAsync_WhenOrderExists_ReturnsOrderDto()
    {
        // Arrange
        var orderNumber = "ORD-20240101120000-1234";
        var orderEntity = TestDataBuilder.CreateOrderEntity(orderNumber: orderNumber);
        
        _mockOrderRepository
            .Setup(x => x.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderEntity);

        // Act
        var result = await _service.GetByOrderNumberAsync(orderNumber);

        // Assert
        result.Should().NotBeNull();
        result!.OrderNumber.Should().Be(orderNumber);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_WhenOrderNotFound_ReturnsNull()
    {
        // Arrange
        var orderNumber = "ORD-NONEXISTENT";
        _mockOrderRepository
            .Setup(x => x.GetByOrderNumberAsync(orderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        // Act
        var result = await _service.GetByOrderNumberAsync(orderNumber);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithValidParameters_ReturnsPagedOrderDtos()
    {
        // Arrange
        var orders = TestDataBuilder.CreateOrderEntityList(5);
        var pagedResult = new PagedResult<OrderEntity>
        {
            Items = orders,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 5,
            TotalPages = 1
        };

        _mockOrderRepository
            .Setup(x => x.GetAllWithDetailsAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetAllAsync(1, 10, null);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(1);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ReturnsFilteredOrders()
    {
        // Arrange
        var orders = TestDataBuilder.CreateOrderEntityList(3);
        var pagedResult = new PagedResult<OrderEntity>
        {
            Items = orders,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 3,
            TotalPages = 1
        };

        _mockOrderRepository
            .Setup(x => x.GetAllWithDetailsAsync(1, 10, OrderStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetAllAsync(1, 10, OrderStatus.Pending);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        _mockOrderRepository.Verify(
            x => x.GetAllWithDetailsAsync(1, 10, OrderStatus.Pending, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoOrders_ReturnsEmptyList()
    {
        // Arrange
        var pagedResult = new PagedResult<OrderEntity>
        {
            Items = new List<OrderEntity>(),
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 0,
            TotalPages = 0
        };

        _mockOrderRepository
            .Setup(x => x.GetAllWithDetailsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrderStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    #endregion

    #region CreateOrderAsync Tests

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_ReturnsCreatedOrderDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId);
        var product = TestDataBuilder.CreateProductEntity(id: productId, price: 50m, stockQuantity: 10);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 2)
            }
        );

        var createdOrder = TestDataBuilder.CreateOrderEntity(
            customerId: customerId,
            subTotal: 100m,
            discountAmount: 10m,
            totalAmount: 90m
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { productId, product } });

        _mockInventoryService
            .Setup(x => x.ValidateBulkStockAvailabilityAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockPricingService
            .Setup(x => x.CalculateDiscount(100m, customer.CustomerType))
            .Returns(10m);

        _mockPricingService
            .Setup(x => x.CalculateTotal(100m, 10m))
            .Returns(90m);

        _mockInventoryService
            .Setup(x => x.ReduceBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(createdOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _service.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customerId);
        result.SubTotal.Should().Be(100m);
        result.DiscountAmount.Should().Be(10m);
        result.TotalAmount.Should().Be(90m);
        result.OrderStatus.Should().Be(OrderStatus.Pending);
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithNonExistentCustomer_ThrowsNotFoundException()
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

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateOrderAsync(request));
    }

    [Fact]
    public async Task CreateOrderAsync_WithEmptyOrderItems_ThrowsArgumentException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>()
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateOrderAsync(request));
    }

    [Fact]
    public async Task CreateOrderAsync_WithInvalidQuantity_ThrowsArgumentException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 0)
            }
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateOrderAsync(request));
    }

    #endregion

    #region CompleteOrderAsync Tests

    [Fact]
    public async Task CompleteOrderAsync_WithPendingOrder_ReturnsCompletedOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            orderStatus: OrderStatus.Pending
        );
        var completedOrder = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            orderStatus: OrderStatus.Completed
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedOrder);

        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedOrder);

        // Act
        var result = await _service.CompleteOrderAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.OrderStatus.Should().Be(OrderStatus.Completed);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteOrderAsync_WithNonExistentOrder_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CompleteOrderAsync(orderId));
    }

    [Fact]
    public async Task CompleteOrderAsync_WithAlreadyCompletedOrder_ThrowsInvalidOrderStatusException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            orderStatus: OrderStatus.Completed
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOrderStatusException>(() => _service.CompleteOrderAsync(orderId));
    }

    [Fact]
    public async Task CompleteOrderAsync_WithCancelledOrder_ThrowsInvalidOrderStatusException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            orderStatus: OrderStatus.Cancelled
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOrderStatusException>(() => _service.CompleteOrderAsync(orderId));
    }

    #endregion

    #region Business Logic Tests - Pricing Calculations

    [Fact]
    public async Task CreateOrderAsync_WithRegularCustomer_CalculatesNoDiscount()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId, customerType: CustomerType.Regular);
        var product = TestDataBuilder.CreateProductEntity(id: productId, price: 100m, stockQuantity: 10);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 1)
            }
        );

        var createdOrder = TestDataBuilder.CreateOrderEntity(
            customerId: customerId,
            subTotal: 100m,
            discountAmount: 0m,
            totalAmount: 100m
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { productId, product } });

        _mockInventoryService
            .Setup(x => x.ValidateBulkStockAvailabilityAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockPricingService
            .Setup(x => x.CalculateDiscount(100m, CustomerType.Regular))
            .Returns(0m);

        _mockPricingService
            .Setup(x => x.CalculateTotal(100m, 0m))
            .Returns(100m);

        _mockInventoryService
            .Setup(x => x.ReduceBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(createdOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _service.CreateOrderAsync(request);

        // Assert
        result.DiscountAmount.Should().Be(0m);
        result.TotalAmount.Should().Be(100m);
        _mockPricingService.Verify(x => x.CalculateDiscount(100m, CustomerType.Regular), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithPremiumCustomer_Calculates5PercentDiscount()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId, customerType: CustomerType.Premium);
        var product = TestDataBuilder.CreateProductEntity(id: productId, price: 100m, stockQuantity: 10);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 1)
            }
        );

        var createdOrder = TestDataBuilder.CreateOrderEntity(
            customerId: customerId,
            subTotal: 100m,
            discountAmount: 5m,
            totalAmount: 95m
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { productId, product } });

        _mockInventoryService
            .Setup(x => x.ValidateBulkStockAvailabilityAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockPricingService
            .Setup(x => x.CalculateDiscount(100m, CustomerType.Premium))
            .Returns(5m);

        _mockPricingService
            .Setup(x => x.CalculateTotal(100m, 5m))
            .Returns(95m);

        _mockInventoryService
            .Setup(x => x.ReduceBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(createdOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _service.CreateOrderAsync(request);

        // Assert
        result.DiscountAmount.Should().Be(5m);
        result.TotalAmount.Should().Be(95m);
        _mockPricingService.Verify(x => x.CalculateDiscount(100m, CustomerType.Premium), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithVIPCustomer_Calculates10PercentDiscount()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId, customerType: CustomerType.VIP);
        var product = TestDataBuilder.CreateProductEntity(id: productId, price: 100m, stockQuantity: 10);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 1)
            }
        );

        var createdOrder = TestDataBuilder.CreateOrderEntity(
            customerId: customerId,
            subTotal: 100m,
            discountAmount: 10m,
            totalAmount: 90m
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { productId, product } });

        _mockInventoryService
            .Setup(x => x.ValidateBulkStockAvailabilityAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockPricingService
            .Setup(x => x.CalculateDiscount(100m, CustomerType.VIP))
            .Returns(10m);

        _mockPricingService
            .Setup(x => x.CalculateTotal(100m, 10m))
            .Returns(90m);

        _mockInventoryService
            .Setup(x => x.ReduceBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(createdOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _service.CreateOrderAsync(request);

        // Assert
        result.DiscountAmount.Should().Be(10m);
        result.TotalAmount.Should().Be(90m);
        _mockPricingService.Verify(x => x.CalculateDiscount(100m, CustomerType.VIP), Times.Once);
    }

    #endregion

    #region Business Logic Tests - Inventory Management

    [Fact]
    public async Task CreateOrderAsync_ValidatesStockAvailabilityBeforeCreation()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId);
        var product = TestDataBuilder.CreateProductEntity(id: productId, price: 50m, stockQuantity: 10);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 5)
            }
        );

        var createdOrder = TestDataBuilder.CreateOrderEntity(
            customerId: customerId,
            subTotal: 250m,
            discountAmount: 0m,
            totalAmount: 250m
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { productId, product } });

        _mockInventoryService
            .Setup(x => x.ValidateBulkStockAvailabilityAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockPricingService
            .Setup(x => x.CalculateDiscount(250m, customer.CustomerType))
            .Returns(0m);

        _mockPricingService
            .Setup(x => x.CalculateTotal(250m, 0m))
            .Returns(250m);

        _mockInventoryService
            .Setup(x => x.ReduceBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(createdOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _service.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        _mockInventoryService.Verify(
            x => x.ValidateBulkStockAvailabilityAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_ReducesInventoryAfterOrderCreation()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId);
        var product = TestDataBuilder.CreateProductEntity(id: productId, price: 50m, stockQuantity: 10);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 3)
            }
        );

        var createdOrder = TestDataBuilder.CreateOrderEntity(
            customerId: customerId,
            subTotal: 150m,
            discountAmount: 0m,
            totalAmount: 150m
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { productId, product } });

        _mockInventoryService
            .Setup(x => x.ValidateBulkStockAvailabilityAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockPricingService
            .Setup(x => x.CalculateDiscount(150m, customer.CustomerType))
            .Returns(0m);

        _mockPricingService
            .Setup(x => x.CalculateTotal(150m, 0m))
            .Returns(150m);

        _mockInventoryService
            .Setup(x => x.ReduceBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(createdOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _service.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        _mockInventoryService.Verify(
            x => x.ReduceBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_RestoresInventoryWhenCancelled()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderItem = TestDataBuilder.CreateOrderItemEntity(
            productId: productId,
            quantity: 5
        );
        var pendingOrder = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            orderStatus: OrderStatus.Pending,
            orderItems: new List<OrderItemEntity> { orderItem }
        );
        var cancelledOrder = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            orderStatus: OrderStatus.Cancelled,
            orderItems: new List<OrderItemEntity> { orderItem }
        );

        var callCount = 0;
        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                return callCount == 1 ? Task.FromResult<OrderEntity?>(pendingOrder) : Task.FromResult<OrderEntity?>(cancelledOrder);
            });

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingOrder);

        _mockInventoryService
            .Setup(x => x.ReturnBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelledOrder);

        // Act
        var result = await _service.CancelOrderAsync(orderId);

        // Assert
        result.OrderStatus.Should().Be(OrderStatus.Cancelled);
        _mockInventoryService.Verify(
            x => x.ReturnBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Business Logic Tests - Multiple Order Items

    [Fact]
    public async Task CreateOrderAsync_WithMultipleOrderItems_CalculatesCorrectSubTotal()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId);
        var product1 = TestDataBuilder.CreateProductEntity(id: productId1, price: 50m, stockQuantity: 10);
        var product2 = TestDataBuilder.CreateProductEntity(id: productId2, price: 30m, stockQuantity: 20);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId1, quantity: 2),
                TestDataBuilder.CreateOrderItemRequest(productId: productId2, quantity: 3)
            }
        );

        var createdOrder = TestDataBuilder.CreateOrderEntity(
            customerId: customerId,
            subTotal: 190m,  // (50*2) + (30*3) = 100 + 90 = 190
            discountAmount: 0m,
            totalAmount: 190m
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity>
            {
                { productId1, product1 },
                { productId2, product2 }
            });

        _mockInventoryService
            .Setup(x => x.ValidateBulkStockAvailabilityAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockPricingService
            .Setup(x => x.CalculateDiscount(190m, customer.CustomerType))
            .Returns(0m);

        _mockPricingService
            .Setup(x => x.CalculateTotal(190m, 0m))
            .Returns(190m);

        _mockInventoryService
            .Setup(x => x.ReduceBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(createdOrder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOrder);

        // Act
        var result = await _service.CreateOrderAsync(request);

        // Assert
        result.SubTotal.Should().Be(190m);
        result.TotalAmount.Should().Be(190m);
    }

    #endregion

    #region Error Scenarios - Product Validation

    [Fact]
    public async Task CreateOrderAsync_WithNonExistentProduct_ThrowsNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var nonExistentProductId = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: nonExistentProductId, quantity: 1)
            }
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Return empty dictionary - product not found
        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity>());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateOrderAsync(request));
    }

    [Fact]
    public async Task CreateOrderAsync_WithMultipleProductsAndOneMissing_ThrowsNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId);
        var product1 = TestDataBuilder.CreateProductEntity(id: productId1, price: 50m, stockQuantity: 10);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId1, quantity: 1),
                TestDataBuilder.CreateOrderItemRequest(productId: productId2, quantity: 1)
            }
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Return only product1, not product2
        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { productId1, product1 } });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateOrderAsync(request));
    }

    #endregion

    #region Error Scenarios - Stock Availability

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_ThrowsInsufficientStockException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId);
        var product = TestDataBuilder.CreateProductEntity(id: productId, price: 50m, stockQuantity: 2);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId, quantity: 5)
            }
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { productId, product } });

        _mockInventoryService
            .Setup(x => x.ValidateBulkStockAvailabilityAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InsufficientStockException(product.Name, product.StockQuantity, 5));

        // Act & Assert
        await Assert.ThrowsAsync<InsufficientStockException>(() => _service.CreateOrderAsync(request));
    }

    [Fact]
    public async Task CreateOrderAsync_WithMultipleProductsAndInsufficientStockOnOne_ThrowsInsufficientStockException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var customer = TestDataBuilder.CreateCustomerEntity(id: customerId);
        var product1 = TestDataBuilder.CreateProductEntity(id: productId1, price: 50m, stockQuantity: 10);
        var product2 = TestDataBuilder.CreateProductEntity(id: productId2, price: 30m, stockQuantity: 2);

        var request = TestDataBuilder.CreateOrderRequest(
            customerId: customerId,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: productId1, quantity: 2),
                TestDataBuilder.CreateOrderItemRequest(productId: productId2, quantity: 5)
            }
        );

        _mockCustomerRepository
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity>
            {
                { productId1, product1 },
                { productId2, product2 }
            });

        _mockInventoryService
            .Setup(x => x.ValidateBulkStockAvailabilityAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InsufficientStockException(product2.Name, product2.StockQuantity, 5));

        // Act & Assert
        await Assert.ThrowsAsync<InsufficientStockException>(() => _service.CreateOrderAsync(request));
    }

    #endregion

    #region Error Scenarios - Order Status Transitions

    [Fact]
    public async Task CancelOrderAsync_WithAlreadyCancelledOrder_ThrowsInvalidOrderStatusException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            orderStatus: OrderStatus.Cancelled
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOrderStatusException>(() => _service.CancelOrderAsync(orderId));
    }

    #endregion

    #region CancelOrderAsync Tests

    [Fact]
    public async Task CancelOrderAsync_WithPendingOrder_ReturnsCancelledOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderItem = TestDataBuilder.CreateOrderItemEntity(
            productId: productId,
            quantity: 2
        );
        var pendingOrder = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            orderStatus: OrderStatus.Pending,
            orderItems: new List<OrderItemEntity> { orderItem }
        );
        var cancelledOrder = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            orderStatus: OrderStatus.Cancelled,
            orderItems: new List<OrderItemEntity> { orderItem }
        );

        // First call returns pending order with details
        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingOrder);

        _mockInventoryService
            .Setup(x => x.ReturnBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Second call returns pending order without details for update
        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingOrder);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelledOrder);

        // Final call returns cancelled order with details
        var callCount = 0;
        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                return callCount == 1 ? Task.FromResult<OrderEntity?>(pendingOrder) : Task.FromResult<OrderEntity?>(cancelledOrder);
            });

        // Act
        var result = await _service.CancelOrderAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.OrderStatus.Should().Be(OrderStatus.Cancelled);
        _mockInventoryService.Verify(
            x => x.ReturnBulkStockAsync(It.IsAny<Dictionary<Guid, int>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_WithNonExistentOrder_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CancelOrderAsync(orderId));
    }

    [Fact]
    public async Task CancelOrderAsync_WithCompletedOrder_ThrowsInvalidOrderStatusException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = TestDataBuilder.CreateOrderEntity(
            id: orderId,
            orderStatus: OrderStatus.Completed
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOrderStatusException>(() => _service.CancelOrderAsync(orderId));
    }

    #endregion
}

