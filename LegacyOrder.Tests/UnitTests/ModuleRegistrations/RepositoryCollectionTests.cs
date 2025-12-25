namespace LegacyOrder.Tests.UnitTests.ModuleRegistrations;

using LegacyOrder.ModuleRegistrations;
using Microsoft.EntityFrameworkCore;

public class RepositoryCollectionTests
{
    [Fact]
    public void AddRepositoryCollection_RegistersDataContext()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var connectionString = "Server=localhost;Database=test;";

        // Act
        services.AddRepositoryCollection(connectionString);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(sd => sd.ServiceType.Name == "DataContext");
        dbContextDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddRepositoryCollection_RegistersProductRepository()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var connectionString = "Server=localhost;Database=test;";

        // Act
        services.AddLogging();
        services.AddRepositoryCollection(connectionString);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<IProductRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<ProductRepository>();
    }

    [Fact]
    public void AddRepositoryCollection_RegistersCustomerRepository()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var connectionString = "Server=localhost;Database=test;";

        // Act
        services.AddLogging();
        services.AddRepositoryCollection(connectionString);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<ICustomerRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<CustomerRepository>();
    }

    [Fact]
    public void AddRepositoryCollection_RegistersOrderRepository()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var connectionString = "Server=localhost;Database=test;";

        // Act
        services.AddLogging();
        services.AddRepositoryCollection(connectionString);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<IOrderRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<OrderRepository>();
    }

    [Fact]
    public void AddRepositoryCollection_RegistersChatRepository()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var connectionString = "Server=localhost;Database=test;";

        // Act
        services.AddLogging();
        services.AddRepositoryCollection(connectionString);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<IChatRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<ChatRepository>();
    }

    [Fact]
    public void AddRepositoryCollection_RegistersRepositoriesAsScoped()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var connectionString = "Server=localhost;Database=test;";

        // Act
        services.AddRepositoryCollection(connectionString);

        // Assert
        var productRepoDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IProductRepository));
        productRepoDescriptor.Should().NotBeNull();
        productRepoDescriptor!.Lifetime.ToString().Should().Be("Scoped");
    }

    [Fact]
    public void AddRepositoryCollection_ReturnsServiceCollection()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var connectionString = "Server=localhost;Database=test;";

        // Act
        var result = services.AddRepositoryCollection(connectionString);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddRepositoryCollection_AllRepositoriesAreRegistered()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var connectionString = "Server=localhost;Database=test;";

        // Act
        services.AddLogging();
        services.AddRepositoryCollection(connectionString);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IProductRepository>().Should().NotBeNull();
        serviceProvider.GetService<ICustomerRepository>().Should().NotBeNull();
        serviceProvider.GetService<IOrderRepository>().Should().NotBeNull();
        serviceProvider.GetService<IChatRepository>().Should().NotBeNull();
    }
}

