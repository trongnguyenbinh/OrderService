using Microsoft.Extensions.Logging;
using Model.Enums;
using Service.Interfaces;

namespace Service.Implementations;

public class PricingService : IPricingService
{
    private readonly ILogger<PricingService> _logger;

    public PricingService(ILogger<PricingService> logger)
    {
        _logger = logger;
    }

    public decimal CalculateDiscount(decimal subTotal, CustomerType customerType)
    {
        _logger.LogInformation("Calculating discount for customer type: {CustomerType}, SubTotal: {SubTotal}", 
            customerType, subTotal);

        decimal discountPercentage = customerType switch
        {
            CustomerType.Regular => 0m,      // No discount
            CustomerType.Premium => 0.05m,   // 5% off
            CustomerType.VIP => 0.10m,       // 10% off
            _ => 0m
        };

        var discountAmount = subTotal * discountPercentage;
        
        _logger.LogInformation("Discount calculated: {DiscountAmount} ({DiscountPercentage}%)", 
            discountAmount, discountPercentage * 100);

        return discountAmount;
    }

    public decimal CalculateTotal(decimal subTotal, decimal discountAmount)
    {
        var total = subTotal - discountAmount;
        
        _logger.LogInformation("Total calculated: {Total} (SubTotal: {SubTotal}, Discount: {DiscountAmount})", 
            total, subTotal, discountAmount);

        return total;
    }
}

