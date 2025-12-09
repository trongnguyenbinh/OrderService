using Domain;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;
using Repository.Interfaces;

namespace LegacyOrder.ModuleRegistrations;

public static class RepositoryCollection
{
    public static IServiceCollection AddRepositoryCollection(this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<DataContext>(option => option.UseNpgsql(connectionString));

        // Register Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();

        return services;
    }
}