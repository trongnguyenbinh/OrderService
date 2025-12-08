using Model.Enums;

namespace Service.Interfaces;

public interface IPricingService
{
    decimal CalculateDiscount(decimal subTotal, CustomerType customerType);
    decimal CalculateTotal(decimal subTotal, decimal discountAmount);
}

