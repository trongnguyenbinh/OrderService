using Domain;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.ModuleRegistrations;

public static class RepositoryCollection
{
    public static IServiceCollection AddRepositoryCollection(this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<DataContext>(option => option.UseNpgsql(connectionString));

        return services;
    }
}