using Domain;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Enums;
using Model.Models;

namespace Repository.Implementations;

public class OrderRepository : IOrderRepository
{
    private readonly DataContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(DataContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting order by ID: {OrderId}", id);

        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order not found with ID: {OrderId}", id);
        }

        return order;
    }

    public async Task<OrderEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting order with details by ID: {OrderId}", id);

        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order not found with ID: {OrderId}", id);
        }

        return order;
    }

    public async Task<IEnumerable<OrderEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all orders");

        var orders = await _context.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} orders", orders.Count);
        return orders;
    }

    public async Task<PagedResult<OrderEntity>> GetAllWithDetailsAsync(int pageNumber = 1, int pageSize = 10, OrderStatus? orderStatus = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all orders with details - Page: {PageNumber}, Size: {PageSize}, Status: {OrderStatus}",
            pageNumber, pageSize, orderStatus?.ToString() ?? "All");

        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsQueryable();

        // Apply status filter if provided
        if (orderStatus.HasValue)
        {
            query = query.Where(o => o.OrderStatus == orderStatus.Value);
            _logger.LogInformation("Filtering orders by status: {OrderStatus}", orderStatus.Value);
        }

        query = query.OrderByDescending(o => o.OrderDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} orders out of {TotalCount}", orders.Count, totalCount);

        return new PagedResult<OrderEntity>
        {
            Items = orders,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<OrderEntity?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting order by order number: {OrderNumber}", orderNumber);

        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order not found with order number: {OrderNumber}", orderNumber);
        }

        return order;
    }

    public async Task<IEnumerable<OrderEntity>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting orders for customer: {CustomerId}", customerId);

        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} orders for customer: {CustomerId}", orders.Count, customerId);
        return orders;
    }

    public async Task<OrderEntity> AddAsync(OrderEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new order with order number: {OrderNumber}", entity.OrderNumber);

        entity.Id = Guid.NewGuid();
        entity.OrderDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.Orders.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully added order with ID: {OrderId}", entity.Id);
        return entity;
    }

    public async Task<OrderEntity> UpdateAsync(OrderEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating order with ID: {OrderId}", entity.Id);

        entity.UpdatedAt = DateTime.UtcNow;

        // Check if the entity is already being tracked
        var trackedEntity = _context.Orders.Local.FirstOrDefault(o => o.Id == entity.Id);

        if (trackedEntity != null)
        {
            // Entity is already tracked, update its properties
            _logger.LogDebug("Order {OrderId} is already tracked, updating properties", entity.Id);
            _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
        }
        else
        {
            // Entity is not tracked, use Update method
            // This handles the case where navigation properties might be present
            _logger.LogDebug("Order {OrderId} is not tracked, updating entity", entity.Id);
            _context.Orders.Update(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated order with ID: {OrderId}", entity.Id);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting order with ID: {OrderId}", id);

        var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order not found with ID: {OrderId}", id);
            return false;
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted order with ID: {OrderId}", id);
        return true;
    }
}

