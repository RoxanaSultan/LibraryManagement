using ServiceLayer.DTOs.Responses;

namespace TestServiceLayer.DTOs;

public class BookListDtoTests
{
    [Fact]
    public void NewBookListDto_HasDefaultValues()
    {
        // Act
        var dto = new BookListDto();

        // Assert
        Assert.Equal(0, dto.BookId);
        Assert.Null(dto.Title);
        Assert.Null(dto.Authors);
        Assert.Null(dto.Domains);
        Assert.Equal(0, dto.TotalAvailableCopies);
        Assert.False(dto.IsAvailableForBorrowing);
    }

    [Fact]
    public void BookListDto_AllPropertiesCanBeSetAndRead()
    {
        // Arrange
        var authors = new List<string> { "Author 1", "Author 2" };
        var domains = new List<string> { "IT", "Software" };

        var dto = new BookListDto
        {
            BookId = 10,
            Title = "Clean Code",
            Authors = authors,
            Domains = domains,
            TotalAvailableCopies = 7,
            IsAvailableForBorrowing = true
        };

        // Assert
        Assert.Equal(10, dto.BookId);
        Assert.Equal("Clean Code", dto.Title);
        Assert.Same(authors, dto.Authors);
        Assert.Same(domains, dto.Domains);
        Assert.Equal(7, dto.TotalAvailableCopies);
        Assert.True(dto.IsAvailableForBorrowing);
    }

    [Fact]
    public void BookListDto_AllowsEmptyLists()
    {
        // Arrange
        var dto = new BookListDto
        {
            Authors = new List<string>(),
            Domains = new List<string>()
        };

        // Assert
        Assert.NotNull(dto.Authors);
        Assert.NotNull(dto.Domains);
        Assert.Empty(dto.Authors);
        Assert.Empty(dto.Domains);
    }

    [Fact]
    public void BookListDto_AuthorsAndDomains_CanBeModifiedAfterSet()
    {
        // Arrange
        var dto = new BookListDto
        {
            Authors = new List<string> { "Author 1" },
            Domains = new List<string> { "IT" }
        };

        // Act
        dto.Authors.Add("Author 2");
        dto.Domains.Add("Software");

        // Assert
        Assert.Equal(2, dto.Authors.Count);
        Assert.Equal(2, dto.Domains.Count);
        Assert.Contains("Author 2", dto.Authors);
        Assert.Contains("Software", dto.Domains);
    }

    [Fact]
    public void BookListDto_AllowsNullCollections()
    {
        // Arrange
        var dto = new BookListDto
        {
            Authors = null,
            Domains = null
        };

        // Assert
        Assert.Null(dto.Authors);
        Assert.Null(dto.Domains);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void BookListDto_TotalAvailableCopies_AllowsVariousValues(int copies)
    {
        var dto = new BookListDto
        {
            TotalAvailableCopies = copies
        };

        Assert.Equal(copies, dto.TotalAvailableCopies);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BookListDto_IsAvailableForBorrowing_CanBeToggled(bool value)
    {
        var dto = new BookListDto
        {
            IsAvailableForBorrowing = value
        };

        Assert.Equal(value, dto.IsAvailableForBorrowing);
    }
}
