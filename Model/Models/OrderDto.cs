namespace Model.Models;

using Model.Enums;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}

