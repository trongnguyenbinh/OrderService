namespace LegacyOrder.Tests.UnitTests.Services;

public class InventoryServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<ILogger<InventoryService>> _mockLogger;
    private readonly InventoryService _inventoryService;

    public InventoryServiceTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockLogger = LoggerFixture.CreateLogger<InventoryService>();
        _inventoryService = new InventoryService(_mockProductRepository.Object, _mockLogger.Object);
    }

    #region ValidateStockAvailabilityAsync Tests

    [Fact]
    public async Task ValidateStockAvailabilityAsync_WithSufficientStock_Succeeds()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = TestDataBuilder.CreateProductEntity(id: productId, stockQuantity: 100);
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await _inventoryService.ValidateStockAvailabilityAsync(productId, 50);
    }

    [Fact]
    public async Task ValidateStockAvailabilityAsync_WithInsufficientStock_ThrowsException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = TestDataBuilder.CreateProductEntity(id: productId, stockQuantity: 30);
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        var act = async () => await _inventoryService.ValidateStockAvailabilityAsync(productId, 50);
        await act.Should().ThrowAsync<InsufficientStockException>();
    }

    [Fact]
    public async Task ValidateStockAvailabilityAsync_WithProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act & Assert
        var act = async () => await _inventoryService.ValidateStockAvailabilityAsync(productId, 50);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region CheckStockAvailabilityAsync Tests

    [Fact]
    public async Task CheckStockAvailabilityAsync_WithSufficientStock_ReturnsTrue()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = TestDataBuilder.CreateProductEntity(id: productId, stockQuantity: 100);
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _inventoryService.CheckStockAvailabilityAsync(productId, 50);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckStockAvailabilityAsync_WithInsufficientStock_ReturnsFalse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = TestDataBuilder.CreateProductEntity(id: productId, stockQuantity: 30);
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _inventoryService.CheckStockAvailabilityAsync(productId, 50);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckStockAvailabilityAsync_WithProductNotFound_ReturnsFalse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _inventoryService.CheckStockAvailabilityAsync(productId, 50);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ReduceStockAsync Tests

    [Fact]
    public async Task ReduceStockAsync_WithValidReduction_UpdatesProductStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = TestDataBuilder.CreateProductEntity(id: productId, stockQuantity: 100);
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _inventoryService.ReduceStockAsync(productId, 30);

        // Assert
        product.StockQuantity.Should().Be(70);
        _mockProductRepository.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReduceStockAsync_WithInsufficientStock_ThrowsException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = TestDataBuilder.CreateProductEntity(id: productId, stockQuantity: 20);
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        var act = async () => await _inventoryService.ReduceStockAsync(productId, 50);
        await act.Should().ThrowAsync<InsufficientStockException>();
    }

    [Fact]
    public async Task ReduceStockAsync_WithProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act & Assert
        var act = async () => await _inventoryService.ReduceStockAsync(productId, 50);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ValidateBulkStockAvailabilityAsync Tests

    [Fact]
    public async Task ValidateBulkStockAvailabilityAsync_WithSufficientStockForAll_Succeeds()
    {
        // Arrange
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var product1 = TestDataBuilder.CreateProductEntity(id: product1Id, stockQuantity: 100);
        var product2 = TestDataBuilder.CreateProductEntity(id: product2Id, stockQuantity: 50);

        var productQuantities = new Dictionary<Guid, int>
        {
            { product1Id, 30 },
            { product2Id, 20 }
        };

        _mockProductRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { product1Id, product1 }, { product2Id, product2 } });

        // Act & Assert
        await _inventoryService.ValidateBulkStockAvailabilityAsync(productQuantities);
    }

    [Fact]
    public async Task ValidateBulkStockAvailabilityAsync_WithInsufficientStockForOne_ThrowsException()
    {
        // Arrange
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var product1 = TestDataBuilder.CreateProductEntity(id: product1Id, stockQuantity: 100);
        var product2 = TestDataBuilder.CreateProductEntity(id: product2Id, stockQuantity: 10);

        var productQuantities = new Dictionary<Guid, int>
        {
            { product1Id, 30 },
            { product2Id, 50 }
        };

        _mockProductRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { product1Id, product1 }, { product2Id, product2 } });

        // Act & Assert
        var act = async () => await _inventoryService.ValidateBulkStockAvailabilityAsync(productQuantities);
        await act.Should().ThrowAsync<InsufficientStockException>();
    }

    [Fact]
    public async Task ValidateBulkStockAvailabilityAsync_WithMissingProduct_ThrowsNotFoundException()
    {
        // Arrange
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var product1 = TestDataBuilder.CreateProductEntity(id: product1Id, stockQuantity: 100);

        var productQuantities = new Dictionary<Guid, int>
        {
            { product1Id, 30 },
            { product2Id, 20 }
        };

        _mockProductRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { product1Id, product1 } });

        // Act & Assert
        var act = async () => await _inventoryService.ValidateBulkStockAvailabilityAsync(productQuantities);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ReduceBulkStockAsync Tests

    [Fact]
    public async Task ReduceBulkStockAsync_WithValidReductions_UpdatesAllProducts()
    {
        // Arrange
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var product1 = TestDataBuilder.CreateProductEntity(id: product1Id, stockQuantity: 100);
        var product2 = TestDataBuilder.CreateProductEntity(id: product2Id, stockQuantity: 50);

        var productQuantities = new Dictionary<Guid, int>
        {
            { product1Id, 30 },
            { product2Id, 20 }
        };

        _mockProductRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { product1Id, product1 }, { product2Id, product2 } });
        _mockProductRepository.Setup(r => r.UpdateRangeAsync(It.IsAny<List<ProductEntity>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _inventoryService.ReduceBulkStockAsync(productQuantities);

        // Assert
        product1.StockQuantity.Should().Be(70);
        product2.StockQuantity.Should().Be(30);
        _mockProductRepository.Verify(r => r.UpdateRangeAsync(It.IsAny<List<ProductEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ReturnBulkStockAsync Tests

    [Fact]
    public async Task ReturnBulkStockAsync_WithValidReturns_UpdatesAllProducts()
    {
        // Arrange
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var product1 = TestDataBuilder.CreateProductEntity(id: product1Id, stockQuantity: 70);
        var product2 = TestDataBuilder.CreateProductEntity(id: product2Id, stockQuantity: 30);

        var productQuantities = new Dictionary<Guid, int>
        {
            { product1Id, 30 },
            { product2Id, 20 }
        };

        _mockProductRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ProductEntity> { { product1Id, product1 }, { product2Id, product2 } });
        _mockProductRepository.Setup(r => r.UpdateRangeAsync(It.IsAny<List<ProductEntity>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _inventoryService.ReturnBulkStockAsync(productQuantities);

        // Assert
        product1.StockQuantity.Should().Be(100);
        product2.StockQuantity.Should().Be(50);
        _mockProductRepository.Verify(r => r.UpdateRangeAsync(It.IsAny<List<ProductEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}

