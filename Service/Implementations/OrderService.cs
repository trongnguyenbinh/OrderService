using AutoMapper;
using Common.Exceptions;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Model.Enums;
using Model.Models;
using Model.RequestModels;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPricingService _pricingService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<OrderService> _logger;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IPricingService pricingService,
        IInventoryService inventoryService,
        ILogger<OrderService> logger,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _pricingService = pricingService;
        _inventoryService = inventoryService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Getting order by ID: {OrderId}", id);

        var order = await _orderRepository.GetByIdWithDetailsAsync(id, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Service: Order not found with ID: {OrderId}", id);
            return null;
        }

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Getting order by order number: {OrderNumber}", orderNumber);

        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Service: Order not found with order number: {OrderNumber}", orderNumber);
            return null;
        }

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<PagedResult<OrderDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, OrderStatus? orderStatus = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Getting all orders - Page: {PageNumber}, Size: {PageSize}, Status: {OrderStatus}",
            pageNumber, pageSize, orderStatus?.ToString() ?? "All");

        var pagedOrders = await _orderRepository.GetAllWithDetailsAsync(pageNumber, pageSize, orderStatus, cancellationToken);

        var orderDtos = _mapper.Map<List<OrderDto>>(pagedOrders.Items);

        _logger.LogInformation("Service: Retrieved {Count} orders with status filter: {OrderStatus}",
            orderDtos.Count, orderStatus?.ToString() ?? "All");

        return new PagedResult<OrderDto>
        {
            Items = orderDtos,
            TotalCount = pagedOrders.TotalCount,
            PageNumber = pagedOrders.PageNumber,
            PageSize = pagedOrders.PageSize
        };
    }

    private string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }

    private void ValidateCreateOrderRequest(CreateOrderRequest request)
    {
        if (request.CustomerId == Guid.Empty)
        {
            throw new ArgumentException("CustomerId is required", nameof(request.CustomerId));
        }

        if (request.OrderItems == null || !request.OrderItems.Any())
        {
            throw new ArgumentException("Order must contain at least one item", nameof(request.OrderItems));
        }

        foreach (var item in request.OrderItems)
        {
            if (item.ProductId == Guid.Empty)
            {
                throw new ArgumentException("ProductId is required for all order items");
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero");
            }
        }
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Creating new order for customer: {CustomerId}", request.CustomerId);

        // Step 1: Validation
        ValidateCreateOrderRequest(request);

        // Step 2: Validate customer exists
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            _logger.LogWarning("Service: Customer not found with ID: {CustomerId}", request.CustomerId);
            throw new NotFoundException("Customer", request.CustomerId);
        }

        // Step 3: Prepare product quantities dictionary for bulk operations
        var productQuantities = request.OrderItems
            .GroupBy(item => item.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(item => item.Quantity)
            );

        // Step 4: Fetch all products in a single query
        var products = await _productRepository.GetByIdsAsync(productQuantities.Keys, cancellationToken);

        // Step 5: Validate all products exist
        foreach (var productId in productQuantities.Keys)
        {
            if (!products.ContainsKey(productId))
            {
                _logger.LogWarning("Service: Product not found with ID: {ProductId}", productId);
                throw new NotFoundException("Product", productId);
            }
        }

        // Step 6: Validate stock availability for all products in bulk
        await _inventoryService.ValidateBulkStockAvailabilityAsync(productQuantities, cancellationToken);

        // Step 7: Create order items and calculate subtotal
        var orderItems = new List<OrderItemEntity>();
        decimal subTotal = 0;

        foreach (var itemRequest in request.OrderItems)
        {
            var product = products[itemRequest.ProductId];

            // Create order item with current product price
            var lineTotal = product.Price * itemRequest.Quantity;
            var orderItem = new OrderItemEntity
            {
                ProductId = itemRequest.ProductId,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.Price,
                LineTotal = lineTotal
            };

            orderItems.Add(orderItem);
            subTotal += lineTotal;
        }

        // Step 8: Calculate pricing
        var discountAmount = _pricingService.CalculateDiscount(subTotal, customer.CustomerType);
        var totalAmount = _pricingService.CalculateTotal(subTotal, discountAmount);

        // Step 9: Create order entity
        var orderNumber = GenerateOrderNumber();
        var orderEntity = new OrderEntity
        {
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            OrderStatus = OrderStatus.Pending,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            OrderItems = orderItems
        };

        // Step 10: Reduce inventory for all products in bulk (single database operation)
        await _inventoryService.ReduceBulkStockAsync(productQuantities, cancellationToken);

        // Step 11: Save order
        var createdOrder = await _orderRepository.AddAsync(orderEntity, cancellationToken);

        _logger.LogInformation("Service: Successfully created order with ID: {OrderId}, OrderNumber: {OrderNumber}",
            createdOrder.Id, createdOrder.OrderNumber);

        // Step 12: Retrieve order with full details for response
        var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(createdOrder.Id, cancellationToken);

        return _mapper.Map<OrderDto>(orderWithDetails);
    }

    public async Task<OrderDto> CompleteOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Completing order with ID: {OrderId}", orderId);

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Service: Order not found with ID: {OrderId}", orderId);
            throw new NotFoundException("Order", orderId);
        }

        // Validate status transition
        if (order.OrderStatus == OrderStatus.Completed)
        {
            _logger.LogWarning("Service: Order {OrderId} is already completed", orderId);
            throw new InvalidOrderStatusException("Order is already completed");
        }

        if (order.OrderStatus == OrderStatus.Cancelled)
        {
            _logger.LogWarning("Service: Cannot complete cancelled order {OrderId}", orderId);
            throw new InvalidOrderStatusException(order.OrderStatus.ToString(), "complete");
        }

        // Update status
        order.OrderStatus = OrderStatus.Completed;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);

        _logger.LogInformation("Service: Successfully completed order with ID: {OrderId}", orderId);

        // Retrieve order with full details for response
        var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);

        return _mapper.Map<OrderDto>(orderWithDetails);
    }

    public async Task<OrderDto> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Service: Cancelling order with ID: {OrderId}", orderId);

        // Retrieve order with full details including order items for stock return
        var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);

        if (orderWithDetails == null)
        {
            _logger.LogWarning("Service: Order not found with ID: {OrderId}", orderId);
            throw new NotFoundException("Order", orderId);
        }

        // Validate status transition - can only cancel from Pending
        if (orderWithDetails.OrderStatus != OrderStatus.Pending)
        {
            _logger.LogWarning("Service: Cannot cancel order {OrderId} with status {Status}", orderId, orderWithDetails.OrderStatus);
            throw new InvalidOrderStatusException(orderWithDetails.OrderStatus.ToString(), "cancel");
        }

        // Prepare product quantities dictionary for bulk stock return
        var productQuantities = orderWithDetails.OrderItems
            .GroupBy(item => item.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(item => item.Quantity)
            );

        _logger.LogInformation("Service: Returning stock for {Count} products from cancelled order {OrderId}",
            productQuantities.Count, orderId);

        // Return stock for all products in bulk
        await _inventoryService.ReturnBulkStockAsync(productQuantities, cancellationToken);

        // Fetch order again WITHOUT details to avoid tracking conflicts
        // This ensures we only update the order entity itself
        var orderToUpdate = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (orderToUpdate == null)
        {
            _logger.LogWarning("Service: Order not found with ID: {OrderId} during update", orderId);
            throw new NotFoundException("Order", orderId);
        }

        // Update order status
        orderToUpdate.OrderStatus = OrderStatus.Cancelled;
        orderToUpdate.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(orderToUpdate, cancellationToken);

        _logger.LogInformation("Service: Successfully cancelled order with ID: {OrderId} and returned stock to inventory", orderId);

        // Retrieve order with full details for response
        var finalOrderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);

        return _mapper.Map<OrderDto>(finalOrderWithDetails);
    }
}

