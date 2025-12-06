namespace Model.RequestModels;

public record CreateOrderRequest(
    string CustomerName,
    string ProductName,
    int Quantity,
    double Price);

