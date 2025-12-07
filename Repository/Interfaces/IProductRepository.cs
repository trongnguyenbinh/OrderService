using Domain.Contractors;
using Domain.Entities;
using Model.Models;
using Model.RequestModels;

namespace Repository.Interfaces;

public interface IProductRepository : IRepository<ProductEntity, Guid>
{
    Task<ProductEntity?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductEntity>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default);
}

