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
}