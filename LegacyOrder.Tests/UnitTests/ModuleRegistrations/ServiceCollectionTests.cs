namespace LegacyOrder.Tests.UnitTests.ModuleRegistrations;

using LegacyOrder.ModuleRegistrations;
using Service.Implementations;
using Service.Interfaces;

public class ServiceCollectionTests
{
    [Fact]
    public void AddServiceCollection_RegistersProductService()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        services.AddServiceCollection();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IProductService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationType.Should().Be(typeof(ProductService));
    }

    [Fact]
    public void AddServiceCollection_RegistersCustomerService()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        services.AddServiceCollection();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(ICustomerService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationType.Should().Be(typeof(CustomerService));
    }

    [Fact]
    public void AddServiceCollection_RegistersOrderService()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        services.AddServiceCollection();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IOrderService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationType.Should().Be(typeof(OrderService));
    }

    [Fact]
    public void AddServiceCollection_RegistersPricingService()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        services.AddServiceCollection();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IPricingService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationType.Should().Be(typeof(PricingService));
    }

    [Fact]
    public void AddServiceCollection_RegistersInventoryService()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        services.AddServiceCollection();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IInventoryService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationType.Should().Be(typeof(InventoryService));
    }

    [Fact]
    public void AddServiceCollection_RegistersChatService()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        services.AddServiceCollection();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IChatService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationType.Should().Be(typeof(ChatService));
    }

    [Fact]
    public void AddServiceCollection_RegistersOpenAIService()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        services.AddServiceCollection();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IOpenAIService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationType.Should().Be(typeof(OpenAIService));
    }

    [Fact]
    public void AddServiceCollection_RegistersServicesAsScoped()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        services.AddServiceCollection();

        // Assert
        var productServiceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IProductService));
        productServiceDescriptor.Should().NotBeNull();
        productServiceDescriptor!.Lifetime.ToString().Should().Be("Scoped");
    }

    [Fact]
    public void AddServiceCollection_ReturnsServiceCollection()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        var result = services.AddServiceCollection();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddServiceCollection_AllServicesAreRegistered()
    {
        // Arrange
        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        services.AddServiceCollection();

        // Assert
        services.FirstOrDefault(sd => sd.ServiceType == typeof(IProductService)).Should().NotBeNull();
        services.FirstOrDefault(sd => sd.ServiceType == typeof(ICustomerService)).Should().NotBeNull();
        services.FirstOrDefault(sd => sd.ServiceType == typeof(IOrderService)).Should().NotBeNull();
        services.FirstOrDefault(sd => sd.ServiceType == typeof(IPricingService)).Should().NotBeNull();
        services.FirstOrDefault(sd => sd.ServiceType == typeof(IInventoryService)).Should().NotBeNull();
        services.FirstOrDefault(sd => sd.ServiceType == typeof(IChatService)).Should().NotBeNull();
        services.FirstOrDefault(sd => sd.ServiceType == typeof(IOpenAIService)).Should().NotBeNull();
    }
}

