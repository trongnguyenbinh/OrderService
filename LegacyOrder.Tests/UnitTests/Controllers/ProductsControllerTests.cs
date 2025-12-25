using LegacyOrder.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace LegacyOrder.Tests.UnitTests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockService;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockService = new Mock<IProductService>();
        _mockLogger = LoggerFixture.CreateLogger<ProductsController>();
        _controller = new ProductsController(_mockService.Object, _mockLogger.Object);
    }

    #region SearchProducts Tests

    [Fact]
    public async Task SearchProducts_WithValidRequest_ReturnsOkWithPagedResults()
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest();
        var pagedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto> { TestDataBuilder.CreateProductDto() },
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1,
            TotalPages = 1
        };

        _mockService
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.SearchProducts(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedData = okResult.Value.Should().BeOfType<PagedResult<ProductDto>>().Subject;
        returnedData.Items.Should().HaveCount(1);
        returnedData.PageNumber.Should().Be(1);
        returnedData.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchProducts_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var request = TestDataBuilder.CreateSearchRequest(
            name: "Test",
            pageNumber: 2,
            pageSize: 20
        );
        var pagedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto>(),
            PageNumber = 2,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0
        };

        _mockService
            .Setup(x => x.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        await _controller.SearchProducts(request, CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.SearchAsync(
                It.Is<ProductSearchRequest>(r =>
                    r.Name == "Test" &&
                    r.PageNumber == 2 &&
                    r.PageSize == 20),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenProductExists_ReturnsOkWithProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productDto = TestDataBuilder.CreateProductDto(id: productId);

        _mockService
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productDto);

        // Act
        var result = await _controller.GetById(productId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        returnedProduct.Id.Should().Be(productId);
    }

    [Fact]
    public async Task GetById_WhenProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockService
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.GetById(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenProductNotFound_ReturnsErrorMessage()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockService
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.GetById(productId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().NotBeNull();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest();
        var createdProduct = TestDataBuilder.CreateProductDto(
            name: request.Name,
            sku: request.SKU
        );

        _mockService
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ProductsController.GetById));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(createdProduct.Id);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedProduct()
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest();
        var createdProduct = TestDataBuilder.CreateProductDto(
            name: request.Name,
            sku: request.SKU,
            price: request.Price
        );

        _mockService
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedProduct = createdResult.Value.Should().BeOfType<ProductDto>().Subject;
        returnedProduct.Name.Should().Be(request.Name);
        returnedProduct.SKU.Should().Be(request.SKU);
        returnedProduct.Price.Should().Be(request.Price);
    }

    [Fact]
    public async Task Create_CallsServiceWithCorrectRequest()
    {
        // Arrange
        var request = TestDataBuilder.CreateProductRequest(
            name: "Test Product",
            sku: "TEST-SKU",
            price: 99.99m
        );
        var createdProduct = TestDataBuilder.CreateProductDto();

        _mockService
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        await _controller.Create(request, CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.CreateAsync(
                It.Is<CreateProductRequest>(r =>
                    r.Name == "Test Product" &&
                    r.SKU == "TEST-SKU" &&
                    r.Price == 99.99m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidRequest_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateProductRequest();
        var updatedProduct = TestDataBuilder.CreateProductDto(
            id: productId,
            name: request.Name,
            sku: request.SKU
        );

        _mockService
            .Setup(x => x.UpdateAsync(productId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.Update(productId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        returnedProduct.Id.Should().Be(productId);
        returnedProduct.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task Update_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = TestDataBuilder.CreateUpdateProductRequest(
            name: "Updated Product",
            sku: "UPDATED-SKU"
        );
        var updatedProduct = TestDataBuilder.CreateProductDto();

        _mockService
            .Setup(x => x.UpdateAsync(productId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProduct);

        // Act
        await _controller.Update(productId, request, CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.UpdateAsync(
                productId,
                It.Is<UpdateProductRequest>(r =>
                    r.Name == "Updated Product" &&
                    r.SKU == "UPDATED-SKU"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenProductExists_ReturnsNoContent()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockService
            .Setup(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockService
            .Setup(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenProductNotFound_ReturnsErrorMessage()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockService
            .Setup(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(productId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_CallsServiceWithCorrectId()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockService
            .Setup(x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _controller.Delete(productId, CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.DeleteAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}


