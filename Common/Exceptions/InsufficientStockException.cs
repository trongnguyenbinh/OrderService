namespace Common.Exceptions;

public class InsufficientStockException : Exception
{
    public string ProductName { get; }
    public int AvailableStock { get; }
    public int RequestedQuantity { get; }

    public InsufficientStockException(string productName, int availableStock, int requestedQuantity)
        : base($"Product '{productName}' has only {availableStock} items in stock, but {requestedQuantity} were requested.")
    {
        ProductName = productName;
        AvailableStock = availableStock;
        RequestedQuantity = requestedQuantity;
    }
}

