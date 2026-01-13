using DomainModel.Entities;

namespace TestDomainModel.Entities;

/// <summary>
/// Testeaza consistenta datelor pentru entitatea Author.
/// </summary>
public class AuthorTests
{
    [Fact]
    public void Author_ShouldInitializeBooksCollection()
    {
        // Act
        var author = new Author();

        // Assert
        Assert.NotNull(author.Books);
    }

    [Theory]
    [InlineData("Mircea Eliade")]
    [InlineData("Mihai Eminescu")]
    public void Author_ShouldStoreNameCorrectly(string name)
    {
        // Act
        var author = new Author { Name = name };

        // Assert
        Assert.Equal(name, author.Name);
    }
}