using Model.Models;
using Model.RequestModels;

namespace Service.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default);
}

