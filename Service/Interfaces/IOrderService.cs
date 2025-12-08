using Model.Enums;
using Model.Models;
using Model.RequestModels;

namespace Service.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, OrderStatus? orderStatus = null, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> CompleteOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderDto> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

