using Domain.Contractors;
using Domain.Entities;
using Model.Enums;
using Model.Models;

namespace Repository.Interfaces;

public interface IOrderRepository : IRepository<OrderEntity, Guid>
{
    Task<OrderEntity?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<OrderEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderEntity>> GetAllWithDetailsAsync(int pageNumber = 1, int pageSize = 10, OrderStatus? orderStatus = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderEntity>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}

