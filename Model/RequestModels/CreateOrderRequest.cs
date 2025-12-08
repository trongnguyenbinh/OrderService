namespace Model.RequestModels;

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public List<OrderItemRequest> OrderItems { get; set; } = new();
}

