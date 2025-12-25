namespace LegacyOrder.Tests.UnitTests.Exceptions;

using Common.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void NotFoundException_WithMessage_CanBeCreated()
    {
        // Arrange
        var message = "Product not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Should().BeOfType<NotFoundException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void NotFoundException_WithEntityNameAndKey_FormatsMessageCorrectly()
    {
        // Arrange
        var entityName = "Product";
        var key = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        exception.Message.Should().Contain(entityName);
        exception.Message.Should().Contain(key.ToString());
        exception.Message.Should().Contain("was not found");
    }

    [Fact]
    public void NotFoundException_WithStringKey_FormatsMessageCorrectly()
    {
        // Arrange
        var entityName = "Customer";
        var key = "CUST-123";

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        exception.Message.Should().Be($"{entityName} with key '{key}' was not found.");
    }

    [Fact]
    public void NotFoundException_WithIntKey_FormatsMessageCorrectly()
    {
        // Arrange
        var entityName = "Order";
        var key = 42;

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        exception.Message.Should().Be($"{entityName} with key '{key}' was not found.");
    }

    [Fact]
    public void NotFoundException_CanBeThrownAndCaught()
    {
        // Arrange
        var message = "Resource not found";

        // Act & Assert
        try
        {
            throw new NotFoundException(message);
        }
        catch (NotFoundException ex)
        {
            ex.Message.Should().Be(message);
        }
    }

    [Fact]
    public void NotFoundException_WithEntityAndKey_CanBeThrownAndCaught()
    {
        // Arrange
        var entityName = "Product";
        var key = Guid.NewGuid();

        // Act & Assert
        try
        {
            throw new NotFoundException(entityName, key);
        }
        catch (NotFoundException ex)
        {
            ex.Message.Should().Contain(entityName);
            ex.Message.Should().Contain(key.ToString());
        }
    }

    [Fact]
    public void NotFoundException_InheritsFromException()
    {
        // Arrange
        var message = "Not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void NotFoundException_WithEmptyMessage_CanBeCreated()
    {
        // Arrange
        var message = string.Empty;

        // Act
        var exception = new NotFoundException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void NotFoundException_WithNullKey_FormatsMessageCorrectly()
    {
        // Arrange
        var entityName = "Item";
        object? key = null;

        // Act
        var exception = new NotFoundException(entityName, key!);

        // Assert
        exception.Message.Should().Contain(entityName);
        exception.Message.Should().Contain("was not found");
    }

    [Fact]
    public void NotFoundException_MultipleInstances_AreIndependent()
    {
        // Arrange
        var exception1 = new NotFoundException("Product", 1);
        var exception2 = new NotFoundException("Customer", 2);

        // Act & Assert
        exception1.Message.Should().NotBe(exception2.Message);
        exception1.Message.Should().Contain("Product");
        exception2.Message.Should().Contain("Customer");
    }

    [Fact]
    public void NotFoundException_WithLongMessage_PreservesFullMessage()
    {
        // Arrange
        var longMessage = new string('a', 1000);

        // Act
        var exception = new NotFoundException(longMessage);

        // Assert
        exception.Message.Should().HaveLength(1000);
        exception.Message.Should().Be(longMessage);
    }
}

