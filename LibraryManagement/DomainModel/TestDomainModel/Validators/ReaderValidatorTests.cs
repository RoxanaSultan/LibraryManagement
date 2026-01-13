using DomainModel.Entities;
using DomainModel.Validators;
using FluentValidation.TestHelper;

namespace TestDomainModel.Validators;

/// <summary>
/// Testeaza regulile de validare pentru cititori.
/// </summary>
public class ReaderValidatorTests
{
    private readonly ReaderValidator _validator = new ReaderValidator();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_FirstName_Is_Invalid(string name)
    {
        var reader = new Reader { FirstName = name };
        var result = _validator.TestValidate(reader);
        result.ShouldHaveValidationErrorFor(r => r.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Have_Error_When_LastName_Is_Invalid(string name)
    {
        var reader = new Reader { LastName = name };
        var result = _validator.TestValidate(reader);
        result.ShouldHaveValidationErrorFor(r => r.LastName);
    }

    [Fact]
    public void Should_Have_Error_When_Both_Contact_Methods_Are_Missing()
    {
        var reader = new Reader { PhoneNumber = null, Email = null };
        var result = _validator.TestValidate(reader);
        result.ShouldHaveValidationErrorFor(r => r); // Regula de cross-property validation
    }

    [Theory]
    [InlineData("0722111222", null)]
    [InlineData(null, "test@library.com")]
    [InlineData("0722111222", "test@library.com")]
    public void Should_Not_Have_Error_When_At_Least_One_Contact_Is_Present(string phone, string email)
    {
        var reader = new Reader
        {
            FirstName = "John",
            LastName = "Doe",
            Address = "Valid Addr",
            PhoneNumber = phone,
            Email = email
        };
        var result = _validator.TestValidate(reader);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Format_Is_Invalid()
    {
        var reader = new Reader { Email = "invalid-email-format" };
        var result = _validator.TestValidate(reader);
        result.ShouldHaveValidationErrorFor(r => r.Email);
    }
}