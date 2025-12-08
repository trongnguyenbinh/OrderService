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
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Register Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IInventoryService, InventoryService>();

        return services;
    }
}

