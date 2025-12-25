namespace LegacyOrder.Tests.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = LoggerFixture.CreateLogger<ProductService>();
        _mapper = AutoMapperFixture.CreateMapper();
        _service = new ProductService(_mockRepository.Object, _mockLogger.Object, _mapper);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProductDto()
    {
        // Arrange
        var productEntity = TestDataBuilder.CreateProductEntity();
        _mockRepository
            .Setup(x => x.GetByIdAsync(productEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productEntity);

        // Act
        var result = await _service.GetByIdAsync(productEntity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productEntity.Id);
        result.Name.Should().Be(productEntity.Name);
        result.SKU.Should().Be(productEntity.SKU);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockRepository
            .Setup(x => x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _service.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ReturnsProductDtoList()
    {
        // Arrange
        var productEntities = TestDataBuilder.CreateProductEntityList(5);
        _mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(productEntities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Should().AllBeOfType<ProductDto>();
    }

    [Fact]
    public async Task GetAllAsync_WhenNoProducts_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductEntity>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedProductDto()
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest();
        var createdEntity = TestDataBuilder.CreateProductEntity(
            name: request.Name,
            sku: request.SKU,
            price: request.Price
        );

        _mockRepository
            .Setup(x => x.GetBySkuAsync(request.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.SKU.Should().Be(request.SKU);
        result.Price.Should().Be(request.Price);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest(name: invalidName!);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Product name is required*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithInvalidSku_ThrowsArgumentException(string? invalidSku)
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest(sku: invalidSku!);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Product SKU is required*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateAsync_WithInvalidPrice_ThrowsArgumentException(decimal invalidPrice)
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest(price: invalidPrice);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Product price must be greater than zero*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateAsync_WithNegativeStockQuantity_ThrowsArgumentException(int invalidStock)
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest(stockQuantity: invalidStock);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Stock quantity cannot be negative*");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSku_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest(sku: "DUPLICATE-SKU");
        var existingProduct = TestDataBuilder.CreateProductEntity(sku: "DUPLICATE-SKU");

        _mockRepository
            .Setup(x => x.GetBySkuAsync(request.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product with SKU 'DUPLICATE-SKU' already exists*");
    }

    [Fact]
    public async Task CreateAsync_WithValidStockQuantityZero_Succeeds()
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest(stockQuantity: 0);
        var createdEntity = TestDataBuilder.CreateProductEntity(stockQuantity: 0);

        _mockRepository
            .Setup(x => x.GetBySkuAsync(request.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.StockQuantity.Should().Be(0);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ReturnsUpdatedProductDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateProductRequest();
        var existingProduct = TestDataBuilder.CreateProductEntity(id: productId, sku: "OLD-SKU");
        var updatedEntity = TestDataBuilder.CreateProductEntity(
            id: productId,
            name: request.Name,
            sku: request.SKU
        );

        _mockRepository
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _mockRepository
            .Setup(x => x.GetBySkuAsync(request.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEntity);

        // Act
        var result = await _service.UpdateAsync(productId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.SKU.Should().Be(request.SKU);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateProductRequest();

        _mockRepository
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(productId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Product with ID {productId} not found*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateProductRequest(name: invalidName!);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(productId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Product name is required*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_WithInvalidSku_ThrowsArgumentException(string? invalidSku)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateProductRequest(sku: invalidSku!);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(productId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Product SKU is required*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task UpdateAsync_WithInvalidPrice_ThrowsArgumentException(decimal invalidPrice)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateProductRequest(price: invalidPrice);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(productId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Product price must be greater than zero*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task UpdateAsync_WithNegativeStockQuantity_ThrowsArgumentException(int invalidStock)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateProductRequest(stockQuantity: invalidStock);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(productId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Stock quantity cannot be negative*");
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateSku_ThrowsInvalidOperationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateProductRequest(sku: "DUPLICATE-SKU");
        var existingProduct = TestDataBuilder.CreateProductEntity(id: productId, sku: "OLD-SKU");
        var conflictingProduct = TestDataBuilder.CreateProductEntity(id: Guid.NewGuid(), sku: "DUPLICATE-SKU");

        _mockRepository
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _mockRepository
            .Setup(x => x.GetBySkuAsync(request.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingProduct);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(productId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("SKU 'DUPLICATE-SKU' is already used by another product*");
    }

    [Fact]
    public async Task UpdateAsync_WithSameSkuAsCurrentProduct_Succeeds()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateProductRequest(sku: "SAME-SKU");
        var existingProduct = TestDataBuilder.CreateProductEntity(id: productId, sku: "SAME-SKU");
        var updatedEntity = TestDataBuilder.CreateProductEntity(id: productId, sku: "SAME-SKU");

        _mockRepository
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _mockRepository
            .Setup(x => x.GetBySkuAsync(request.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEntity);

        // Act
        var result = await _service.UpdateAsync(productId, request);

        // Assert
        result.Should().NotBeNull();
        result.SKU.Should().Be("SAME-SKU");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenProductExists_ReturnsTrue()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository
            .Setup(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(productId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductNotFound_ReturnsFalse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository
            .Setup(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(productId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetBySkuAsync Tests

    [Fact]
    public async Task GetBySkuAsync_WhenSkuExists_ReturnsProductDto()
    {
        // Arrange
        var productEntity = TestDataBuilder.CreateProductEntity(sku: "TEST-SKU");
        _mockRepository
            .Setup(x => x.GetBySkuAsync("TEST-SKU", It.IsAny<CancellationToken>()))
            .ReturnsAsync(productEntity);

        // Act
        var result = await _service.GetBySkuAsync("TEST-SKU");

        // Assert
        result.Should().NotBeNull();
        result!.SKU.Should().Be("TEST-SKU");
    }

    [Fact]
    public async Task GetBySkuAsync_WhenSkuNotFound_ReturnsNull()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetBySkuAsync("NON-EXISTENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _service.GetBySkuAsync("NON-EXISTENT");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_WithValidRequest_ReturnsPagedResults()
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest();
        var pagedResult = TestDataBuilder.CreatePagedResult();

        _mockRepository
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(pagedResult.Items.Count());
        result.PageNumber.Should().Be(pagedResult.PageNumber);
        result.TotalCount.Should().Be(pagedResult.TotalCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task SearchAsync_WithInvalidPageNumber_ThrowsArgumentException(int invalidPageNumber)
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest(pageNumber: invalidPageNumber);

        // Act
        Func<Task> act = async () => await _service.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Page number must be greater than or equal to 1*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task SearchAsync_WithInvalidPageSize_ThrowsArgumentException(int invalidPageSize)
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest(pageSize: invalidPageSize);

        // Act
        Func<Task> act = async () => await _service.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Page size must be greater than 0*");
    }

    [Fact]
    public async Task SearchAsync_WithPageSizeGreaterThan100_ThrowsArgumentException()
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest(pageSize: 101);

        // Act
        Func<Task> act = async () => await _service.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Page size cannot exceed 100*");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("ascending")]
    [InlineData("descending")]
    public async Task SearchAsync_WithInvalidSortDirection_ThrowsArgumentException(string invalidDirection)
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest(sortDirection: invalidDirection);

        // Act
        Func<Task> act = async () => await _service.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Sort direction must be 'asc' or 'desc'*");
    }

    [Theory]
    [InlineData("name")]
    [InlineData("sku")]
    [InlineData("category")]
    [InlineData("invalid")]
    public async Task SearchAsync_WithInvalidSortBy_ThrowsArgumentException(string invalidSortBy)
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest(sortBy: invalidSortBy);

        // Act
        Func<Task> act = async () => await _service.SearchAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Sort by must be either 'Price' or 'StockQuantity'*");
    }

    [Theory]
    [InlineData("price")]
    [InlineData("Price")]
    [InlineData("PRICE")]
    public async Task SearchAsync_WithValidSortByPrice_Succeeds(string sortBy)
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest(sortBy: sortBy);
        var pagedResult = TestDataBuilder.CreatePagedResult();

        _mockRepository
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("stockquantity")]
    [InlineData("StockQuantity")]
    [InlineData("STOCKQUANTITY")]
    public async Task SearchAsync_WithValidSortByStockQuantity_Succeeds(string sortBy)
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest(sortBy: sortBy);
        var pagedResult = TestDataBuilder.CreatePagedResult();

        _mockRepository
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    public async Task SearchAsync_WithValidSortDirection_Succeeds(string sortDirection)
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest(sortDirection: sortDirection);
        var pagedResult = TestDataBuilder.CreatePagedResult();

        _mockRepository
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_MapsEntityDtosCorrectly()
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest();
        var entities = TestDataBuilder.CreateProductEntityList(3);
        var pagedResult = new PagedResult<ProductEntity>
        {
            Items = entities,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 3,
            TotalPages = 1
        };

        _mockRepository
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().AllBeOfType<ProductDto>();
        result.Items.Should().HaveCount(3);

        var dtoList = result.Items.ToList();
        var entityList = entities.ToList();

        for (int i = 0; i < 3; i++)
        {
            dtoList[i].Id.Should().Be(entityList[i].Id);
            dtoList[i].Name.Should().Be(entityList[i].Name);
            dtoList[i].SKU.Should().Be(entityList[i].SKU);
        }
    }

    #endregion
}




