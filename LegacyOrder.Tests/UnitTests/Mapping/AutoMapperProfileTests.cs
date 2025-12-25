using LegacyOrder.ModuleRegistrations;

namespace LegacyOrder.Tests.UnitTests.Mapping;

public class AutoMapperProfileTests
{
    private readonly IMapper _mapper;

    public AutoMapperProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        });

        _mapper = config.CreateMapper();
    }

    [Fact]
    public void AutoMapperProfile_ConfigurationIsValid()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        });

        // Act & Assert
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_ProductEntityToProductDto_MapsAllProperties()
    {
        // Arrange
        var entity = TestDataBuilder.CreateProductEntity(
            id: Guid.NewGuid(),
            name: "Test Product",
            description: "Test Description",
            sku: "TEST-SKU",
            price: 99.99m,
            stockQuantity: 50,
            category: "Electronics",
            isActive: true
        );

        // Act
        var dto = _mapper.Map<ProductDto>(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(entity.Id);
        dto.Name.Should().Be(entity.Name);
        dto.Description.Should().Be(entity.Description);
        dto.SKU.Should().Be(entity.SKU);
        dto.Price.Should().Be(entity.Price);
        dto.StockQuantity.Should().Be(entity.StockQuantity);
        dto.Category.Should().Be(entity.Category);
        dto.IsActive.Should().Be(entity.IsActive);
        dto.CreatedAt.Should().Be(entity.CreatedAt);
        dto.UpdatedAt.Should().Be(entity.UpdatedAt);
    }

    [Fact]
    public void Map_ProductDtoToProductEntity_MapsAllProperties()
    {
        // Arrange
        var dto = TestDataBuilder.CreateProductDto(
            id: Guid.NewGuid(),
            name: "Test Product",
            description: "Test Description",
            sku: "TEST-SKU",
            price: 99.99m,
            stockQuantity: 50,
            category: "Electronics",
            isActive: true
        );

        // Act
        var entity = _mapper.Map<ProductEntity>(dto);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().Be(dto.Id);
        entity.Name.Should().Be(dto.Name);
        entity.Description.Should().Be(dto.Description);
        entity.SKU.Should().Be(dto.SKU);
        entity.Price.Should().Be(dto.Price);
        entity.StockQuantity.Should().Be(dto.StockQuantity);
        entity.Category.Should().Be(dto.Category);
        entity.IsActive.Should().Be(dto.IsActive);
        entity.CreatedAt.Should().Be(dto.CreatedAt);
        entity.UpdatedAt.Should().Be(dto.UpdatedAt);
    }

    [Fact]
    public void Map_CreateProductRequestToProductEntity_MapsAllProperties()
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest(
            name: "Test Product",
            description: "Test Description",
            sku: "TEST-SKU",
            price: 99.99m,
            stockQuantity: 50,
            category: "Electronics",
            isActive: true
        );

        // Act
        var entity = _mapper.Map<ProductEntity>(request);

        // Assert
        entity.Should().NotBeNull();
        entity.Name.Should().Be(request.Name);
        entity.Description.Should().Be(request.Description);
        entity.SKU.Should().Be(request.SKU);
        entity.Price.Should().Be(request.Price);
        entity.StockQuantity.Should().Be(request.StockQuantity);
        entity.Category.Should().Be(request.Category);
        entity.IsActive.Should().Be(request.IsActive);
    }

    [Fact]
    public void Map_CreateProductRequestToProductEntity_IgnoresIdCreatedAtUpdatedAt()
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest();

        // Act
        var entity = _mapper.Map<ProductEntity>(request);

        // Assert
        // Id should be empty (default Guid)
        entity.Id.Should().Be(Guid.Empty);

        // CreatedAt and UpdatedAt are set to DateTime.UtcNow by ProductEntity's property initializers
        // AutoMapper's Ignore() doesn't override these default values, it just doesn't map from the source
        // So we verify they are set to a recent time (within the last minute)
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Map_UpdateProductRequestToProductEntity_MapsAllProperties()
    {
        // Arrange
        var request = TestDataBuilder.CreateUpdateProductRequest(
            name: "Updated Product",
            description: "Updated Description",
            sku: "UPDATED-SKU",
            price: 149.99m,
            stockQuantity: 75,
            category: "Books",
            isActive: false
        );

        // Act
        var entity = _mapper.Map<ProductEntity>(request);

        // Assert
        entity.Should().NotBeNull();
        entity.Name.Should().Be(request.Name);
        entity.Description.Should().Be(request.Description);
        entity.SKU.Should().Be(request.SKU);
        entity.Price.Should().Be(request.Price);
        entity.StockQuantity.Should().Be(request.StockQuantity);
        entity.Category.Should().Be(request.Category);
        entity.IsActive.Should().Be(request.IsActive);
    }
}

