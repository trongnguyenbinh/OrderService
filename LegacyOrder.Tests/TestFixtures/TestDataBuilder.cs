namespace LegacyOrder.Tests.TestFixtures;

using Model.Enums;

public class TestDataBuilder
{
    public static ProductEntity CreateProductEntity(
        Guid? id = null,
        string name = "Test Product",
        string? description = "Test Description",
        string sku = "TEST-SKU-001",
        decimal price = 99.99m,
        int stockQuantity = 10,
        string? category = "Electronics",
        bool isActive = true,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new ProductEntity
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            SKU = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category,
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow
        };
    }

    public static CreateProductRequest CreateProductRequest(
        string name = "Test Product",
        string? description = "Test Description",
        string sku = "TEST-SKU-001",
        decimal price = 99.99m,
        int stockQuantity = 10,
        string? category = "Electronics",
        bool isActive = true)
    {
        return new CreateProductRequest
        {
            Name = name,
            Description = description,
            SKU = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category,
            IsActive = isActive
        };
    }

    public static UpdateProductRequest CreateUpdateProductRequest(
        string name = "Updated Product",
        string? description = "Updated Description",
        string sku = "UPDATED-SKU-001",
        decimal price = 149.99m,
        int stockQuantity = 20,
        string? category = "Electronics",
        bool isActive = true)
    {
        return new UpdateProductRequest
        {
            Name = name,
            Description = description,
            SKU = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category,
            IsActive = isActive
        };
    }

    public static ProductSearchRequest CreateSearchRequest(
        string? name = null,
        string? description = null,
        string? sku = null,
        string? category = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        string sortDirection = "asc")
    {
        return new ProductSearchRequest
        {
            Name = name,
            Description = description,
            SKU = sku,
            Category = category,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
    }

    public static ProductDto CreateProductDto(
        Guid? id = null,
        string name = "Test Product",
        string? description = "Test Description",
        string sku = "TEST-SKU-001",
        decimal price = 99.99m,
        int stockQuantity = 10,
        string? category = "Electronics",
        bool isActive = true,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new ProductDto
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            SKU = sku,
            Price = price,
            StockQuantity = stockQuantity,
            Category = category,
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow
        };
    }

    public static List<ProductEntity> CreateProductEntityList(int count = 5)
    {
        var products = new List<ProductEntity>();
        for (int i = 1; i <= count; i++)
        {
            products.Add(CreateProductEntity(
                name: $"Product {i}",
                sku: $"SKU-{i:D3}",
                price: 10m * i,
                stockQuantity: i * 5
            ));
        }
        return products;
    }

    public static PagedResult<ProductEntity> CreatePagedResult(
        List<ProductEntity>? items = null,
        int pageNumber = 1,
        int pageSize = 10,
        int totalCount = 0)
    {
        items ??= CreateProductEntityList(pageSize);
        totalCount = totalCount == 0 ? items.Count : totalCount;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<ProductEntity>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public static CustomerEntity CreateCustomerEntity(
        Guid? id = null,
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com",
        string? phoneNumber = "555-123-4567",
        CustomerType customerType = CustomerType.Regular,
        bool isActive = true,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new CustomerEntity
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            CustomerType = customerType,
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow
        };
    }

    public static CreateCustomerRequest CreateCustomerRequest(
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com",
        string? phoneNumber = "555-123-4567",
        CustomerType customerType = CustomerType.Regular,
        bool isActive = true)
    {
        return new CreateCustomerRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            CustomerType = customerType,
            IsActive = isActive
        };
    }

    public static UpdateCustomerRequest CreateUpdateCustomerRequest(
        string firstName = "Jane",
        string lastName = "Smith",
        string email = "jane.smith@example.com",
        string? phoneNumber = "555-567-8901",
        CustomerType customerType = CustomerType.Premium,
        bool isActive = true)
    {
        return new UpdateCustomerRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            CustomerType = customerType,
            IsActive = isActive
        };
    }

    public static CustomerDto CreateCustomerDto(
        Guid? id = null,
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com",
        string? phoneNumber = "555-123-4567",
        CustomerType customerType = CustomerType.Regular,
        bool isActive = true,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new CustomerDto
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            CustomerType = customerType,
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow
        };
    }

    public static CustomerSearchRequest CreateCustomerSearchRequest(
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? phoneNumber = null,
        CustomerType? customerType = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        string sortDirection = "asc")
    {
        return new CustomerSearchRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            CustomerType = customerType,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
    }

    public static List<CustomerEntity> CreateCustomerEntityList(int count = 5)
    {
        var customers = new List<CustomerEntity>();
        for (int i = 1; i <= count; i++)
        {
            customers.Add(CreateCustomerEntity(
                firstName: $"Customer{i}",
                lastName: $"Last{i}",
                email: $"customer{i}@example.com",
                phoneNumber: $"555-{i:D3}-{i:D4}",
                customerType: (CustomerType)(i % 3)
            ));
        }
        return customers;
    }

    // Order-related test data builders
    public static OrderEntity CreateOrderEntity(
        Guid? id = null,
        string orderNumber = "ORD-20240101120000-1234",
        Guid? customerId = null,
        OrderStatus orderStatus = OrderStatus.Pending,
        decimal subTotal = 100m,
        decimal discountAmount = 10m,
        decimal totalAmount = 90m,
        DateTime? orderDate = null,
        DateTime? updatedAt = null,
        List<OrderItemEntity>? orderItems = null)
    {
        return new OrderEntity
        {
            Id = id ?? Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerId = customerId ?? Guid.NewGuid(),
            OrderStatus = orderStatus,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            OrderDate = orderDate ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow,
            OrderItems = orderItems ?? new List<OrderItemEntity>()
        };
    }

    public static CreateOrderRequest CreateOrderRequest(
        Guid? customerId = null,
        List<OrderItemRequest>? orderItems = null)
    {
        return new CreateOrderRequest
        {
            CustomerId = customerId ?? Guid.NewGuid(),
            OrderItems = orderItems ?? new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 2 }
            }
        };
    }

    public static OrderItemEntity CreateOrderItemEntity(
        Guid? id = null,
        Guid? orderId = null,
        Guid? productId = null,
        int quantity = 2,
        decimal unitPrice = 50m,
        decimal lineTotal = 100m)
    {
        return new OrderItemEntity
        {
            Id = id ?? Guid.NewGuid(),
            OrderId = orderId ?? Guid.NewGuid(),
            ProductId = productId ?? Guid.NewGuid(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = lineTotal
        };
    }

    public static OrderItemRequest CreateOrderItemRequest(
        Guid? productId = null,
        int quantity = 2)
    {
        return new OrderItemRequest
        {
            ProductId = productId ?? Guid.NewGuid(),
            Quantity = quantity
        };
    }

    public static OrderDto CreateOrderDto(
        Guid? id = null,
        string orderNumber = "ORD-20240101120000-1234",
        Guid? customerId = null,
        string customerName = "John Doe",
        string customerEmail = "john.doe@example.com",
        CustomerType customerType = CustomerType.Regular,
        OrderStatus orderStatus = OrderStatus.Pending,
        decimal subTotal = 100m,
        decimal discountAmount = 10m,
        decimal totalAmount = 90m,
        DateTime? orderDate = null,
        DateTime? updatedAt = null,
        List<OrderItemDto>? orderItems = null)
    {
        return new OrderDto
        {
            Id = id ?? Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerId = customerId ?? Guid.NewGuid(),
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CustomerType = customerType,
            OrderStatus = orderStatus,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            OrderDate = orderDate ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow,
            OrderItems = orderItems ?? new List<OrderItemDto>()
        };
    }

    public static OrderItemDto CreateOrderItemDto(
        Guid? id = null,
        Guid? orderId = null,
        Guid? productId = null,
        string productName = "Test Product",
        string productSKU = "TEST-SKU",
        int quantity = 2,
        decimal unitPrice = 50m,
        decimal lineTotal = 100m)
    {
        return new OrderItemDto
        {
            Id = id ?? Guid.NewGuid(),
            OrderId = orderId ?? Guid.NewGuid(),
            ProductId = productId ?? Guid.NewGuid(),
            ProductName = productName,
            ProductSKU = productSKU,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = lineTotal
        };
    }

    public static List<OrderEntity> CreateOrderEntityList(int count = 5, Guid? customerId = null)
    {
        var orders = new List<OrderEntity>();
        var defaultCustomerId = customerId ?? Guid.NewGuid();
        for (int i = 1; i <= count; i++)
        {
            orders.Add(CreateOrderEntity(
                orderNumber: $"ORD-20240101120000-{i:D4}",
                customerId: defaultCustomerId,
                subTotal: 100m * i,
                discountAmount: 10m * i,
                totalAmount: 90m * i,
                orderDate: DateTime.UtcNow.AddDays(-count + i)
            ));
        }
        return orders;
    }

    public static ChatSessionEntity CreateChatSessionEntity(
        Guid? id = null,
        string userFingerprint = "test-fingerprint-123",
        DateTime? createdAt = null,
        DateTime? lastActivityAt = null,
        bool isActive = true)
    {
        return new ChatSessionEntity
        {
            Id = id ?? Guid.NewGuid(),
            UserFingerprint = userFingerprint,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            LastActivityAt = lastActivityAt ?? DateTime.UtcNow,
            IsActive = isActive,
            Messages = new List<ChatMessageEntity>()
        };
    }

    public static ChatMessageEntity CreateChatMessageEntity(
        Guid? id = null,
        Guid? sessionId = null,
        string role = "user",
        string content = "Test message",
        DateTime? createdAt = null,
        int? promptTokens = null,
        int? completionTokens = null,
        int? totalTokens = null,
        string? modelUsed = null,
        string? toolCalled = null,
        int? responseTimeMs = null)
    {
        return new ChatMessageEntity
        {
            Id = id ?? Guid.NewGuid(),
            SessionId = sessionId ?? Guid.NewGuid(),
            Role = role,
            Content = content,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = totalTokens,
            ModelUsed = modelUsed,
            ToolCalled = toolCalled,
            ResponseTimeMs = responseTimeMs
        };
    }
}

