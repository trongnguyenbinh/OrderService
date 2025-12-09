using Service.Implementations;
using Service.Interfaces;

namespace LegacyOrder.ModuleRegistrations;

public static class ServiceCollection
{
    public static IServiceCollection AddServiceCollection(this IServiceCollection services)
    {
        // Register Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IOpenAIService, OpenAIService>();

        return services;
    }
}

