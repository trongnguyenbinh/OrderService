namespace LegacyOrder.Tests.UnitTests.Models;

using Model.Models;

public class PagedResultTests
{
    [Fact]
    public void PagedResult_HasPreviousPage_ReturnsTrueWhenPageNumberGreaterThanOne()
    {
        // Arrange
        var result = new PagedResult<string> { PageNumber = 2 };

        // Act & Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void PagedResult_HasPreviousPage_ReturnsFalseWhenPageNumberIsOne()
    {
        // Arrange
        var result = new PagedResult<string> { PageNumber = 1 };

        // Act & Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void PagedResult_HasNextPage_ReturnsTrueWhenPageNumberLessThanTotalPages()
    {
        // Arrange
        var result = new PagedResult<string> { PageNumber = 1, TotalPages = 3 };

        // Act & Assert
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void PagedResult_HasNextPage_ReturnsFalseWhenPageNumberEqualsOrGreaterThanTotalPages()
    {
        // Arrange
        var result = new PagedResult<string> { PageNumber = 3, TotalPages = 3 };

        // Act & Assert
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PagedResult_WithSinglePage_HasNoPreviousOrNextPage()
    {
        // Arrange
        var result = new PagedResult<string> { PageNumber = 1, TotalPages = 1 };

        // Act & Assert
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PagedResult_WithMultiplePages_NavigationPropertiesAreCorrect()
    {
        // Arrange & Act
        var result = new PagedResult<string>
        {
            PageNumber = 2,
            TotalPages = 5
        };

        // Assert
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void PagedResult_MiddlePage_HasBothPreviousAndNextPage()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 5,
            PageSize = 10,
            TotalCount = 100,
            TotalPages = 10
        };

        // Act & Assert
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void PagedResult_LastPage_HasNoPreviousPageWhenPageNumberIsOne()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 10,
            TotalPages = 1
        };

        // Act & Assert
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PagedResult_DefaultInitialization_HasEmptyItems()
    {
        // Arrange & Act
        var result = new PagedResult<string>();

        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.PageNumber.Should().Be(0);
        result.PageSize.Should().Be(0);
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void PagedResult_CanSetAllProperties()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };

        // Act
        var result = new PagedResult<string>
        {
            Items = items,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 25,
            TotalPages = 3
        };

        // Assert
        result.Items.Should().HaveCount(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
    }
}

