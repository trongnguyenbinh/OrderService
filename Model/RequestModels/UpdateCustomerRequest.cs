namespace Model.RequestModels;

using Model.Enums;

public class UpdateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public CustomerType CustomerType { get; set; }
    public bool IsActive { get; set; }
}

