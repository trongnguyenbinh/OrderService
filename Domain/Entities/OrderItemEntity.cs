using System.ComponentModel.DataAnnotations.Schema;
using Domain.Contractors;

namespace Domain.Entities;

[Table("order_items")]
public class OrderItemEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    // Navigation properties
    public OrderEntity Order { get; set; } = null!;
    public ProductEntity Product { get; set; } = null!;
}

