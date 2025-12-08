using Domain.Entities;

namespace Service.Interfaces;

public interface IInventoryService
{
    Task ValidateStockAvailabilityAsync(Guid productId, int requestedQuantity, CancellationToken cancellationToken = default);
    Task ReduceStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> CheckStockAvailabilityAsync(Guid productId, int requestedQuantity, CancellationToken cancellationToken = default);
    Task ValidateBulkStockAvailabilityAsync(Dictionary<Guid, int> productQuantities, CancellationToken cancellationToken = default);
    Task ReduceBulkStockAsync(Dictionary<Guid, int> productQuantities, CancellationToken cancellationToken = default);
    Task ReturnBulkStockAsync(Dictionary<Guid, int> productQuantities, CancellationToken cancellationToken = default);
}

