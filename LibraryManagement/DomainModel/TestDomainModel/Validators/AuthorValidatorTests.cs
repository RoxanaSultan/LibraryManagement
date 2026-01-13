using DomainModel.Entities;
using DomainModel.Validators;
using FluentValidation.TestHelper;

namespace TestDomainModel.Validators;

/// <summary>
/// Unit tests for the Author entity validator.
/// </summary>
public class AuthorValidatorTests
{
    private readonly AuthorValidator _validator = new AuthorValidator();

    [Theory]
    [InlineData("Andrew Tanenbaum")]
    [InlineData("Robert C. Martin")]
    [InlineData("J.K. Rowling")]
    public void Should_Not_Have_Error_When_Author_Name_Is_Valid(string name)
    {
        var author = new Author { Name = name };
        var result = _validator.TestValidate(author);
        result.ShouldNotHaveValidationErrorFor(a => a.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("\t\n")]
    public void Should_Have_Error_When_Author_Name_Is_Empty(string name)
    {
        var author = new Author { Name = name };
        var result = _validator.TestValidate(author);
        result.ShouldHaveValidationErrorFor(a => a.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Author_Name_Exceeds_150_Characters()
    {
        // Arrange: Create a string with 151 characters
        var longName = new string('X', 151);
        var author = new Author { Name = longName };

        // Act
        var result = _validator.TestValidate(author);

        // Assert
        result.ShouldHaveValidationErrorFor(a => a.Name);
    }

    [Theory]
    [InlineData("A")] // Minimum valid length
    [InlineData("D'Artagnan")] // Special characters
    public void Should_Be_Valid_With_Specific_Name_Formats(string name)
    {
        var author = new Author { Name = name };
        var result = _validator.TestValidate(author);
        result.ShouldNotHaveAnyValidationErrors();
    }
}