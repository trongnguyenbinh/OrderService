using Domain.Contractors;
using LegacyOrder.Tests.TestFixtures;
using Model.Enums;

namespace LegacyOrder.Tests.UnitTests.Domain.Entities;

public class CustomerEntityTests
{
    [Fact]
    public void CustomerEntity_ImplementsIEntity()
    {
        // Arrange & Act
        var customer = new CustomerEntity();

        // Assert
        customer.Should().BeAssignableTo<IEntity<Guid>>();
    }

    [Fact]
    public void CustomerEntity_CanSetAndGetId()
    {
        // Arrange
        var customer = new CustomerEntity();
        var expectedId = Guid.NewGuid();

        // Act
        customer.Id = expectedId;

        // Assert
        customer.Id.Should().Be(expectedId);
    }

    [Fact]
    public void CustomerEntity_CanSetAndGetFirstName()
    {
        // Arrange
        var customer = new CustomerEntity();
        var expectedFirstName = "John";

        // Act
        customer.FirstName = expectedFirstName;

        // Assert
        customer.FirstName.Should().Be(expectedFirstName);
    }

    [Fact]
    public void CustomerEntity_CanSetAndGetLastName()
    {
        // Arrange
        var customer = new CustomerEntity();
        var expectedLastName = "Doe";

        // Act
        customer.LastName = expectedLastName;

        // Assert
        customer.LastName.Should().Be(expectedLastName);
    }

    [Fact]
    public void CustomerEntity_CanSetAndGetEmail()
    {
        // Arrange
        var customer = new CustomerEntity();
        var expectedEmail = "john.doe@example.com";

        // Act
        customer.Email = expectedEmail;

        // Assert
        customer.Email.Should().Be(expectedEmail);
    }

    [Fact]
    public void CustomerEntity_CanSetAndGetPhoneNumber()
    {
        // Arrange
        var customer = new CustomerEntity();
        var expectedPhoneNumber = "555-123-4567";

        // Act
        customer.PhoneNumber = expectedPhoneNumber;

        // Assert
        customer.PhoneNumber.Should().Be(expectedPhoneNumber);
    }

    [Fact]
    public void CustomerEntity_CanSetAndGetCustomerType()
    {
        // Arrange
        var customer = new CustomerEntity();
        var expectedCustomerType = CustomerType.Premium;

        // Act
        customer.CustomerType = expectedCustomerType;

        // Assert
        customer.CustomerType.Should().Be(expectedCustomerType);
    }

    [Fact]
    public void CustomerEntity_DefaultCustomerTypeIsRegular()
    {
        // Arrange & Act
        var customer = new CustomerEntity();

        // Assert
        customer.CustomerType.Should().Be(CustomerType.Regular);
    }

    [Fact]
    public void CustomerEntity_CanSetAndGetIsActive()
    {
        // Arrange
        var customer = new CustomerEntity();

        // Act
        customer.IsActive = false;

        // Assert
        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CustomerEntity_DefaultIsActiveIsTrue()
    {
        // Arrange & Act
        var customer = new CustomerEntity();

        // Assert
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CustomerEntity_CanSetAndGetCreatedAt()
    {
        // Arrange
        var customer = new CustomerEntity();
        var expectedDate = DateTime.UtcNow;

        // Act
        customer.CreatedAt = expectedDate;

        // Assert
        customer.CreatedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void CustomerEntity_CanSetAndGetUpdatedAt()
    {
        // Arrange
        var customer = new CustomerEntity();
        var expectedDate = DateTime.UtcNow;

        // Act
        customer.UpdatedAt = expectedDate;

        // Assert
        customer.UpdatedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void CustomerEntity_AllPropertiesCanBeSetViaBuilder()
    {
        // Arrange & Act
        var customer = TestDataBuilder.CreateCustomerEntity(
            id: Guid.NewGuid(),
            firstName: "John",
            lastName: "Doe",
            email: "john.doe@example.com",
            phoneNumber: "555-123-4567",
            customerType: CustomerType.Premium,
            isActive: true,
            createdAt: DateTime.UtcNow.AddDays(-1),
            updatedAt: DateTime.UtcNow
        );

        // Assert
        customer.Should().NotBeNull();
        customer.FirstName.Should().Be("John");
        customer.LastName.Should().Be("Doe");
        customer.Email.Should().Be("john.doe@example.com");
        customer.PhoneNumber.Should().Be("555-123-4567");
        customer.CustomerType.Should().Be(CustomerType.Premium);
        customer.IsActive.Should().BeTrue();
    }
}

