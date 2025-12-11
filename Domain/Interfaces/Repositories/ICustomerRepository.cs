using Domain.Contractors;
using Domain.Entities;
using Model.Models;
using Model.RequestModels;

namespace Domain.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<CustomerEntity, Guid>
{
    Task<CustomerEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerEntity>> SearchAsync(CustomerSearchRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderEntity>> GetCustomerOrdersAsync(Guid customerId, CancellationToken cancellationToken = default);
}

