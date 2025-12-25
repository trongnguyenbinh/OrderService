namespace LegacyOrder.Tests.UnitTests.Models;

using Model.Models;
using Model.Enums;

public class CustomerDtoTests
{
    [Fact]
    public void CustomerDto_CanBeCreatedWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john@example.com";
        var phoneNumber = "555-1234";
        var customerType = CustomerType.Premium;
        var isActive = true;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var dto = new CustomerDto
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            CustomerType = customerType,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.FirstName.Should().Be(firstName);
        dto.LastName.Should().Be(lastName);
        dto.Email.Should().Be(email);
        dto.PhoneNumber.Should().Be(phoneNumber);
        dto.CustomerType.Should().Be(customerType);
        dto.IsActive.Should().BeTrue();
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void CustomerDto_DefaultInitialization_HasDefaultValues()
    {
        // Arrange & Act
        var dto = new CustomerDto();

        // Assert
        dto.Id.Should().Be(Guid.Empty);
        dto.FirstName.Should().Be(string.Empty);
        dto.LastName.Should().Be(string.Empty);
        dto.Email.Should().Be(string.Empty);
        dto.PhoneNumber.Should().BeNull();
        dto.IsActive.Should().BeFalse();
        dto.CreatedAt.Should().Be(default(DateTime));
        dto.UpdatedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void CustomerDto_CanBeModifiedAfterCreation()
    {
        // Arrange
        var dto = new CustomerDto { Id = Guid.NewGuid(), FirstName = "John" };
        var newFirstName = "Jane";
        var newEmail = "jane@example.com";

        // Act
        dto.FirstName = newFirstName;
        dto.Email = newEmail;

        // Assert
        dto.FirstName.Should().Be(newFirstName);
        dto.Email.Should().Be(newEmail);
    }

    [Fact]
    public void CustomerDto_WithNullablePhoneNumber_CanBeNull()
    {
        // Arrange & Act
        var dto = new CustomerDto
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = null
        };

        // Assert
        dto.PhoneNumber.Should().BeNull();
    }

    [Fact]
    public void CustomerDto_MultipleInstances_AreIndependent()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var dto1 = new CustomerDto { Id = id1, FirstName = "John", Email = "john@example.com" };
        var dto2 = new CustomerDto { Id = id2, FirstName = "Jane", Email = "jane@example.com" };

        // Assert
        dto1.Id.Should().NotBe(dto2.Id);
        dto1.FirstName.Should().NotBe(dto2.FirstName);
        dto1.Email.Should().NotBe(dto2.Email);
    }
}

