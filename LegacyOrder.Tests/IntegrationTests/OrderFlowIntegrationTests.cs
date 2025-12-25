using Domain;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.Tests.IntegrationTests;

public class OrderFlowIntegrationTests : IDisposable
{
    private readonly DataContext _context;
    private readonly OrderRepository _orderRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly ProductRepository _productRepository;
    private readonly OrderService _orderService;
    private readonly PricingService _pricingService;
    private readonly InventoryService _inventoryService;
    private readonly IMapper _mapper;

    public OrderFlowIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mapper = AutoMapperFixture.CreateMapper();
        
        var orderRepositoryLogger = LoggerFixture.CreateNullLogger<OrderRepository>();
        var customerRepositoryLogger = LoggerFixture.CreateNullLogger<CustomerRepository>();
        var productRepositoryLogger = LoggerFixture.CreateNullLogger<ProductRepository>();
        var pricingServiceLogger = LoggerFixture.CreateNullLogger<PricingService>();
        var inventoryServiceLogger = LoggerFixture.CreateNullLogger<InventoryService>();
        var orderServiceLogger = LoggerFixture.CreateNullLogger<OrderService>();

        _orderRepository = new OrderRepository(_context, orderRepositoryLogger);
        _customerRepository = new CustomerRepository(_context, customerRepositoryLogger);
        _productRepository = new ProductRepository(_context, productRepositoryLogger);
        _pricingService = new PricingService(pricingServiceLogger);
        _inventoryService = new InventoryService(_productRepository, inventoryServiceLogger);
        _orderService = new OrderService(
            _orderRepository,
            _customerRepository,
            _productRepository,
            _pricingService,
            _inventoryService,
            orderServiceLogger,
            _mapper
        );
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Order Creation and Retrieval Flow Tests

    [Fact]
    public async Task OrderFlow_CreateOrderAndRetrieve_Success()
    {
        // Arrange - Create customer and product
        var customer = TestDataBuilder.CreateCustomerEntity(customerType: CustomerType.Regular);
        var product = TestDataBuilder.CreateProductEntity(price: 100m, stockQuantity: 10);
        
        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 2)
            }
        );

        // Act - Create order
        var createdOrder = await _orderService.CreateOrderAsync(createRequest);

        // Act - Retrieve order
        var retrievedOrder = await _orderService.GetByIdAsync(createdOrder.Id);

        // Assert
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.Id.Should().Be(createdOrder.Id);
        retrievedOrder.CustomerId.Should().Be(customer.Id);
        retrievedOrder.OrderStatus.Should().Be(OrderStatus.Pending);
        retrievedOrder.SubTotal.Should().Be(200m);
        retrievedOrder.DiscountAmount.Should().Be(0m);
        retrievedOrder.TotalAmount.Should().Be(200m);
    }

    [Fact]
    public async Task OrderFlow_CreateOrderWithMultipleItems_CalculatesCorrectTotal()
    {
        // Arrange - Create customer and products
        var customer = TestDataBuilder.CreateCustomerEntity(customerType: CustomerType.Premium);
        var product1 = TestDataBuilder.CreateProductEntity(price: 100m, stockQuantity: 10);
        var product2 = TestDataBuilder.CreateProductEntity(price: 50m, stockQuantity: 20);
        
        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product1);
        await _productRepository.AddAsync(product2);

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product1.Id, quantity: 2),
                TestDataBuilder.CreateOrderItemRequest(productId: product2.Id, quantity: 3)
            }
        );

        // Act - Create order
        var createdOrder = await _orderService.CreateOrderAsync(createRequest);

        // Assert
        createdOrder.SubTotal.Should().Be(350m);  // (100*2) + (50*3) = 200 + 150 = 350
        createdOrder.DiscountAmount.Should().Be(17.5m);  // 5% of 350
        createdOrder.TotalAmount.Should().Be(332.5m);  // 350 - 17.5
    }

    #endregion

    #region Order Status Transition Flow Tests

    [Fact]
    public async Task OrderFlow_CreateCompleteOrder_Success()
    {
        // Arrange - Create customer and product
        var customer = TestDataBuilder.CreateCustomerEntity();
        var product = TestDataBuilder.CreateProductEntity(price: 100m, stockQuantity: 10);
        
        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 1)
            }
        );

        // Act - Create order
        var createdOrder = await _orderService.CreateOrderAsync(createRequest);
        createdOrder.OrderStatus.Should().Be(OrderStatus.Pending);

        // Act - Complete order
        var completedOrder = await _orderService.CompleteOrderAsync(createdOrder.Id);

        // Assert
        completedOrder.OrderStatus.Should().Be(OrderStatus.Completed);
        
        // Verify order is persisted as completed
        var retrievedOrder = await _orderService.GetByIdAsync(createdOrder.Id);
        retrievedOrder!.OrderStatus.Should().Be(OrderStatus.Completed);
    }

    #endregion

    #region Order Cancellation and Inventory Flow Tests

    [Fact]
    public async Task OrderFlow_CreateCancelOrder_RestoresInventory()
    {
        // Arrange - Create customer and product
        var customer = TestDataBuilder.CreateCustomerEntity();
        var product = TestDataBuilder.CreateProductEntity(price: 100m, stockQuantity: 10);

        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        var initialStock = product.StockQuantity;

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 3)
            }
        );

        // Act - Create order (reduces inventory)
        var createdOrder = await _orderService.CreateOrderAsync(createRequest);

        // Verify inventory was reduced
        var productAfterOrder = await _productRepository.GetByIdAsync(product.Id);
        productAfterOrder!.StockQuantity.Should().Be(initialStock - 3);

        // Act - Cancel order (restores inventory)
        var cancelledOrder = await _orderService.CancelOrderAsync(createdOrder.Id);

        // Assert
        cancelledOrder.OrderStatus.Should().Be(OrderStatus.Cancelled);

        // Verify inventory was restored
        var productAfterCancel = await _productRepository.GetByIdAsync(product.Id);
        productAfterCancel!.StockQuantity.Should().Be(initialStock);
    }

    [Fact]
    public async Task OrderFlow_CreateCancelCreateAgain_Success()
    {
        // Arrange - Create customer and product
        var customer = TestDataBuilder.CreateCustomerEntity();
        var product = TestDataBuilder.CreateProductEntity(price: 100m, stockQuantity: 5);

        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 3)
            }
        );

        // Act - Create first order
        var firstOrder = await _orderService.CreateOrderAsync(createRequest);
        firstOrder.OrderStatus.Should().Be(OrderStatus.Pending);

        // Act - Cancel first order
        var cancelledOrder = await _orderService.CancelOrderAsync(firstOrder.Id);
        cancelledOrder.OrderStatus.Should().Be(OrderStatus.Cancelled);

        // Act - Create second order with same product
        var secondOrder = await _orderService.CreateOrderAsync(createRequest);

        // Assert - Second order should succeed because inventory was restored
        secondOrder.OrderStatus.Should().Be(OrderStatus.Pending);
        secondOrder.Id.Should().NotBe(firstOrder.Id);
    }

    #endregion

    #region Order Retrieval and Filtering Tests

    [Fact]
    public async Task OrderFlow_GetAllOrders_ReturnsAllOrders()
    {
        // Arrange - Create customer and products
        var customer = TestDataBuilder.CreateCustomerEntity();
        var product = TestDataBuilder.CreateProductEntity(price: 100m, stockQuantity: 20);

        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        // Create multiple orders
        var createRequest1 = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 1)
            }
        );

        var createRequest2 = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 2)
            }
        );

        // Act - Create orders
        await _orderService.CreateOrderAsync(createRequest1);
        await _orderService.CreateOrderAsync(createRequest2);

        // Act - Get all orders
        var allOrders = await _orderService.GetAllAsync(1, 10, null);

        // Assert
        allOrders.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        allOrders.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task OrderFlow_GetOrdersByStatus_ReturnsFilteredOrders()
    {
        // Arrange - Create customer and product
        var customer = TestDataBuilder.CreateCustomerEntity();
        var product = TestDataBuilder.CreateProductEntity(price: 100m, stockQuantity: 20);

        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 1)
            }
        );

        // Act - Create and complete an order
        var order = await _orderService.CreateOrderAsync(createRequest);
        await _orderService.CompleteOrderAsync(order.Id);

        // Act - Get completed orders
        var completedOrders = await _orderService.GetAllAsync(1, 10, OrderStatus.Completed);

        // Assert
        completedOrders.Items.Should().HaveCountGreaterThanOrEqualTo(1);
        completedOrders.Items.All(o => o.OrderStatus == OrderStatus.Completed).Should().BeTrue();
    }

    #endregion

    #region PricingService Integration Tests

    [Fact]
    public async Task OrderFlow_PricingService_RegularCustomerReceivesNoDiscount()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity(customerType: CustomerType.Regular);
        var product = TestDataBuilder.CreateProductEntity(price: 100m, stockQuantity: 10);

        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 2)
            }
        );

        // Act
        var order = await _orderService.CreateOrderAsync(createRequest);

        // Assert - Regular customer should have 0% discount
        var subTotal = 100m * 2; // 2 items at 100 each
        order.DiscountAmount.Should().Be(0m);
        order.TotalAmount.Should().Be(subTotal);
    }

    [Fact]
    public async Task OrderFlow_PricingService_PremiumCustomerReceives5PercentDiscount()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity(customerType: CustomerType.Premium);
        var product = TestDataBuilder.CreateProductEntity(price: 100m, stockQuantity: 10);

        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 2)
            }
        );

        // Act
        var order = await _orderService.CreateOrderAsync(createRequest);

        // Assert - Premium customer should have 5% discount
        var subTotal = 100m * 2; // 2 items at 100 each
        var expectedDiscount = subTotal * 0.05m; // 5% discount
        order.DiscountAmount.Should().Be(expectedDiscount);
        order.TotalAmount.Should().Be(subTotal - expectedDiscount);
    }

    [Fact]
    public async Task OrderFlow_PricingService_VIPCustomerReceives10PercentDiscount()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity(customerType: CustomerType.VIP);
        var product = TestDataBuilder.CreateProductEntity(price: 100m, stockQuantity: 10);

        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 2)
            }
        );

        // Act
        var order = await _orderService.CreateOrderAsync(createRequest);

        // Assert - VIP customer should have 10% discount
        var subTotal = 100m * 2; // 2 items at 100 each
        var expectedDiscount = subTotal * 0.10m; // 10% discount
        order.DiscountAmount.Should().Be(expectedDiscount);
        order.TotalAmount.Should().Be(subTotal - expectedDiscount);
    }

    #endregion

    #region InventoryService Integration Tests

    [Fact]
    public async Task OrderFlow_InventoryService_ReducesStockOnOrderCreation()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var product = TestDataBuilder.CreateProductEntity(price: 50m, stockQuantity: 100);

        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        var initialStock = product.StockQuantity;

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 30)
            }
        );

        // Act
        await _orderService.CreateOrderAsync(createRequest);

        // Assert - Stock should be reduced
        var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
        updatedProduct!.StockQuantity.Should().Be(initialStock - 30);
    }

    [Fact]
    public async Task OrderFlow_InventoryService_RestoresStockOnOrderCancellation()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var product = TestDataBuilder.CreateProductEntity(price: 50m, stockQuantity: 100);

        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product);

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product.Id, quantity: 30)
            }
        );

        var order = await _orderService.CreateOrderAsync(createRequest);

        // Act - Cancel the order
        await _orderService.CancelOrderAsync(order.Id);

        // Assert - Stock should be restored
        var finalProduct = await _productRepository.GetByIdAsync(product.Id);
        finalProduct!.StockQuantity.Should().Be(100); // Back to original
    }

    [Fact]
    public async Task OrderFlow_InventoryService_HandlesMultipleProductsInOrder()
    {
        // Arrange
        var customer = TestDataBuilder.CreateCustomerEntity();
        var product1 = TestDataBuilder.CreateProductEntity(price: 50m, stockQuantity: 100);
        var product2 = TestDataBuilder.CreateProductEntity(price: 75m, stockQuantity: 50);

        await _customerRepository.AddAsync(customer);
        await _productRepository.AddAsync(product1);
        await _productRepository.AddAsync(product2);

        var createRequest = TestDataBuilder.CreateOrderRequest(
            customerId: customer.Id,
            orderItems: new List<OrderItemRequest>
            {
                TestDataBuilder.CreateOrderItemRequest(productId: product1.Id, quantity: 20),
                TestDataBuilder.CreateOrderItemRequest(productId: product2.Id, quantity: 15)
            }
        );

        // Act
        await _orderService.CreateOrderAsync(createRequest);

        // Assert - Both products should have reduced stock
        var updatedProduct1 = await _productRepository.GetByIdAsync(product1.Id);
        var updatedProduct2 = await _productRepository.GetByIdAsync(product2.Id);

        updatedProduct1!.StockQuantity.Should().Be(80); // 100 - 20
        updatedProduct2!.StockQuantity.Should().Be(35); // 50 - 15
    }

    #endregion
}

