namespace Domain.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using Model.Enums;
using Domain.Contractors;

[Table("customers")]
public class CustomerEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Regular;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

