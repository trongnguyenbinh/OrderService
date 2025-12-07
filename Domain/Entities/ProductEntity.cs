namespace Domain.Entities;

using Domain.Contractors;

public class ProductEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double Price { get; set; }
}