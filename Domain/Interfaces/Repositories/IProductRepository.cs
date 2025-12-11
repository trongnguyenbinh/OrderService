using Domain.Contractors;
using Domain.Entities;
using Model.Models;
using Model.RequestModels;

namespace Domain.Interfaces.Repositories;

public interface IProductRepository : IRepository<ProductEntity, Guid>
{
    Task<ProductEntity?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductEntity>> SearchForAIToolAsync(ProductSearchRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductEntity>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, ProductEntity>> GetByIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<ProductEntity> products, CancellationToken cancellationToken = default);
}

