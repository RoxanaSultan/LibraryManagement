using DomainModel.Entities;

namespace TestDomainModel.Entities;

public class BookTests
{
    [Fact]
    public void NewBook_ShouldInitializeCollections()
    {
        // Act
        var book = new Book();

        // Assert
        Assert.NotNull(book.Authors);
        Assert.NotNull(book.ExplicitDomains);
        Assert.NotNull(book.Editions);
    }

    [Fact]
    public void Book_ShouldLinkToAuthorsCorrectly()
    {
        // Arrange
        var book = new Book { Title = "C# Basics" };
        var author = new Author { Name = "John Smith" };

        // Act
        book.Authors.Add(author);

        // Assert
        Assert.Contains(author, book.Authors);
    }

    [Fact]
    public void Book_ShouldHoldMultipleEditions()
    {
        // Arrange
        var book = new Book { Title = "C# Basics" };

        var ed1 = new Edition
        {
            Publisher = "Editura All",
            Year = 2000,
            EditionNumber = "1",
            PageCount = 300,
            BookType = "Paperback",
            InitialStock = 100,
            CurrentStock = 100,
            ReadingRoomOnlyCount = 0
        };

        var ed2 = new Edition
        {
            Publisher = "Editura All",
            Year = 2010,
            EditionNumber = "3",
            PageCount = 320,
            BookType = "Hardcover",
            InitialStock = 80,
            CurrentStock = 80,
            ReadingRoomOnlyCount = 0
        };

        // Act
        book.Editions.Add(ed1);
        book.Editions.Add(ed2);

        // Assert
        Assert.Equal(2, book.Editions.Count);
        Assert.Contains(ed1, book.Editions);
        Assert.Contains(ed2, book.Editions);
    }

}