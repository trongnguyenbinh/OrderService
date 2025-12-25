using Model.Enums;
using Service.Implementations;
using Service.Interfaces;

namespace LegacyOrder.Tests.UnitTests.Services;

public class PricingServiceTests
{
    private readonly Mock<ILogger<PricingService>> _mockLogger;
    private readonly PricingService _pricingService;

    public PricingServiceTests()
    {
        _mockLogger = LoggerFixture.CreateLogger<PricingService>();
        _pricingService = new PricingService(_mockLogger.Object);
    }

    #region CalculateDiscount Tests

    [Fact]
    public void CalculateDiscount_WithRegularCustomer_ReturnsZeroDiscount()
    {
        // Arrange
        var subTotal = 100m;
        var customerType = CustomerType.Regular;

        // Act
        var discount = _pricingService.CalculateDiscount(subTotal, customerType);

        // Assert
        discount.Should().Be(0m);
    }

    [Fact]
    public void CalculateDiscount_WithPremiumCustomer_Returns5PercentDiscount()
    {
        // Arrange
        var subTotal = 100m;
        var customerType = CustomerType.Premium;

        // Act
        var discount = _pricingService.CalculateDiscount(subTotal, customerType);

        // Assert
        discount.Should().Be(5m); // 5% of 100
    }

    [Fact]
    public void CalculateDiscount_WithVIPCustomer_Returns10PercentDiscount()
    {
        // Arrange
        var subTotal = 100m;
        var customerType = CustomerType.VIP;

        // Act
        var discount = _pricingService.CalculateDiscount(subTotal, customerType);

        // Assert
        discount.Should().Be(10m); // 10% of 100
    }

    [Fact]
    public void CalculateDiscount_WithPremiumCustomer50SubTotal_Returns2Point5Discount()
    {
        // Act
        var discount = _pricingService.CalculateDiscount(50m, CustomerType.Premium);

        // Assert
        discount.Should().Be(2.5m);
    }

    [Fact]
    public void CalculateDiscount_WithPremiumCustomer200SubTotal_Returns10Discount()
    {
        // Act
        var discount = _pricingService.CalculateDiscount(200m, CustomerType.Premium);

        // Assert
        discount.Should().Be(10m);
    }

    [Fact]
    public void CalculateDiscount_WithVIPCustomer1000SubTotal_Returns100Discount()
    {
        // Act
        var discount = _pricingService.CalculateDiscount(1000m, CustomerType.VIP);

        // Assert
        discount.Should().Be(100m);
    }

    [Fact]
    public void CalculateDiscount_WithVIPCustomer75SubTotal_Returns7Point5Discount()
    {
        // Act
        var discount = _pricingService.CalculateDiscount(75m, CustomerType.VIP);

        // Assert
        discount.Should().Be(7.5m);
    }

    [Fact]
    public void CalculateDiscount_WithZeroSubTotal_ReturnsZeroDiscount()
    {
        // Arrange
        var subTotal = 0m;
        var customerType = CustomerType.VIP;

        // Act
        var discount = _pricingService.CalculateDiscount(subTotal, customerType);

        // Assert
        discount.Should().Be(0m);
    }

    #endregion

    #region CalculateTotal Tests

    [Fact]
    public void CalculateTotal_WithValidInputs_ReturnsCorrectTotal()
    {
        // Arrange
        var subTotal = 100m;
        var discountAmount = 10m;

        // Act
        var total = _pricingService.CalculateTotal(subTotal, discountAmount);

        // Assert
        total.Should().Be(90m);
    }

    [Fact]
    public void CalculateTotal_WithNoDiscount_ReturnsSubTotal()
    {
        // Arrange
        var subTotal = 100m;
        var discountAmount = 0m;

        // Act
        var total = _pricingService.CalculateTotal(subTotal, discountAmount);

        // Assert
        total.Should().Be(100m);
    }

    [Fact]
    public void CalculateTotal_With100SubTotal5Discount_Returns95()
    {
        // Act
        var total = _pricingService.CalculateTotal(100m, 5m);

        // Assert
        total.Should().Be(95m);
    }

    [Fact]
    public void CalculateTotal_With200SubTotal20Discount_Returns180()
    {
        // Act
        var total = _pricingService.CalculateTotal(200m, 20m);

        // Assert
        total.Should().Be(180m);
    }

    [Fact]
    public void CalculateTotal_With50SubTotal2Point5Discount_Returns47Point5()
    {
        // Act
        var total = _pricingService.CalculateTotal(50m, 2.5m);

        // Assert
        total.Should().Be(47.5m);
    }

    [Fact]
    public void CalculateTotal_With1000SubTotal100Discount_Returns900()
    {
        // Act
        var total = _pricingService.CalculateTotal(1000m, 100m);

        // Assert
        total.Should().Be(900m);
    }

    [Fact]
    public void CalculateTotal_WithZeroSubTotal_ReturnsNegativeDiscount()
    {
        // Arrange
        var subTotal = 0m;
        var discountAmount = 10m;

        // Act
        var total = _pricingService.CalculateTotal(subTotal, discountAmount);

        // Assert
        total.Should().Be(-10m);
    }

    #endregion
}

