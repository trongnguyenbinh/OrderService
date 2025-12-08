namespace Model.RequestModels;

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

