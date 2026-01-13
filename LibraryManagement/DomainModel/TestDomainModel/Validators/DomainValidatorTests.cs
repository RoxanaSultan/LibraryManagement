using DomainModel.Entities;
using DomainModel.Validators;
using FluentValidation.TestHelper;

namespace TestDomainModel.Validators;

/// <summary>
/// Unit tests for the Domain entity validator.
/// </summary>
public class DomainValidatorTests
{
    private readonly DomainValidator _validator = new DomainValidator();

    [Theory]
    [InlineData("Science")]
    [InlineData("Computer Science")]
    [InlineData("Algorithms & Data Structures")]
    public void Should_Not_Have_Error_When_Name_Is_Valid(string name)
    {
        var domain = new Domain { Name = name };
        var result = _validator.TestValidate(domain);
        result.ShouldNotHaveValidationErrorFor(d => d.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Name_Is_Empty(string name)
    {
        var domain = new Domain { Name = name };
        var result = _validator.TestValidate(domain);
        result.ShouldHaveValidationErrorFor(d => d.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_100_Characters()
    {
        // Arrange: Create a string with 101 characters
        var longName = new string('A', 101);
        var domain = new Domain { Name = longName };

        // Act
        var result = _validator.TestValidate(domain);

        // Assert
        result.ShouldHaveValidationErrorFor(d => d.Name)
              .WithErrorMessage("The length of 'Name' must be 100 characters or fewer. You entered 101 characters.");
    }

    [Fact]
    public void Should_Be_Valid_When_Name_Is_Exactly_100_Characters()
    {
        var domain = new Domain { Name = new string('B', 100) };
        var result = _validator.TestValidate(domain);
        result.ShouldNotHaveValidationErrorFor(d => d.Name);
    }
}