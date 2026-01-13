using DomainModel.Entities;

namespace TestDomainModel.Entities;

public class ReaderTests
{
    [Fact]
    public void Reader_ShouldCombineFullNameCorrectly()
    {
        // Arrange
        var reader = new Reader { FirstName = "John", LastName = "Doe" };

        // Act
        string fullName = $"{reader.FirstName} {reader.LastName}";

        // Assert
        Assert.Equal("John Doe", fullName);
    }

    [Theory]
    [InlineData("ACC123", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Reader_ShouldHaveValidAccountId(string accountId, bool expected)
    {
        var reader = new Reader { AccountId = accountId };
        bool hasAccount = !string.IsNullOrEmpty(reader.AccountId);
        Assert.Equal(expected, hasAccount);
    }
}