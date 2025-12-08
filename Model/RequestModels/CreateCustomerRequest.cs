namespace Model.RequestModels;

using Model.Enums;

public class CreateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Regular;
    public bool IsActive { get; set; } = true;
}

