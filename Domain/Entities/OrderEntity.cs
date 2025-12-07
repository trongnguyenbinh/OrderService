using System.ComponentModel.DataAnnotations.Schema;
using Domain.Contractors;

namespace Domain.Entities;

[Table("orders")]
public class OrderEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string? CustomerName { get; set; }
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
}