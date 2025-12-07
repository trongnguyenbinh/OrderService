using Repository.Implementations;
using Repository.Interfaces;
using Service.Implementations;
using Service.Interfaces;

namespace LegacyOrder.ModuleRegistrations;

public static class ServiceCollection
{
    public static IServiceCollection AddServiceCollection(this IServiceCollection services)
    {
        // Register Repositories
        services.AddScoped<IProductRepository, ProductRepository>();

        // Register Services
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}

