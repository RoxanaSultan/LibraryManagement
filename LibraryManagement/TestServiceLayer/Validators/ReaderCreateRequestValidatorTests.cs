using FluentValidation.TestHelper;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.Validators;

namespace TestServiceLayer.Validators;

public class ReaderCreateRequestValidatorTests
{
    private readonly ReaderCreateRequestValidator _validator = new();

    private static ReaderCreateRequest ValidRequest() => new()
    {
        FirstName = "Ana",
        LastName = "Pop",
        Address = "Strada 1",
        AccountId = "ACC-1",
        PhoneNumber = "0712345678",
        Email = null
    };

    [Fact]
    public void Valid_WhenPhoneProvided_EmailMissing()
    {
        var model = ValidRequest();
        model.Email = null;
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Valid_WhenEmailProvided_PhoneMissing()
    {
        var model = ValidRequest();
        model.PhoneNumber = null;
        model.Email = "ana@test.com";

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Valid_WhenBothPhoneAndEmailProvided()
    {
        var model = ValidRequest();
        model.Email = "ana@test.com";

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_WhenBothPhoneAndEmailMissing()
    {
        var model = ValidRequest();
        model.PhoneNumber = null;
        model.Email = null;

        var result = _validator.TestValidate(model);

        // asta e regula "Must" pe obiect
        Assert.Contains(result.Errors, e => e.ErrorMessage ==
            "Trebuie specificat cel putin un numar de telefon sau o adresa de email.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Invalid_WhenFirstNameEmpty(string firstName)
    {
        var model = ValidRequest();
        model.FirstName = firstName;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("Prenumele este obligatoriu.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Invalid_WhenLastNameEmpty(string lastName)
    {
        var model = ValidRequest();
        model.LastName = lastName;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Numele este obligatoriu.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Invalid_WhenAddressEmpty(string address)
    {
        var model = ValidRequest();
        model.Address = address;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Address)
              .WithErrorMessage("Adresa este obligatorie.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Invalid_WhenAccountIdEmpty(string accountId)
    {
        var model = ValidRequest();
        model.AccountId = accountId;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.AccountId)
              .WithErrorMessage("ID-ul contului este obligatoriu.");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("ana@")]
    [InlineData("@test.com")]
    [InlineData("ana.test.com")]
    [InlineData("ana@@test.com")]
    [InlineData("ana@.com")]
    [InlineData("ana@com.")]
    public void Invalid_WhenEmailProvidedButFormatIsBad(string email)
    {
        var model = ValidRequest();
        model.PhoneNumber = null;
        model.Email = email;

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Valid_WhenEmailEmptyString_ButPhoneProvided()
    {
        // Email empty => regula EmailAddress nu ruleaza (When)
        var model = ValidRequest();
        model.Email = "";
        model.PhoneNumber = "0712345678";

        var result = _validator.TestValidate(model);

        // trebuie să fie valid, fiindcă telefonul e ok și email-ul nu e verificat
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_WhenEmailEmptyString_AndPhoneMissing()
    {
        // Email empty => When nu ruleaza, dar Must trebuie sa pice
        var model = ValidRequest();
        model.PhoneNumber = null;
        model.Email = "";

        var result = _validator.TestValidate(model);

        Assert.Contains(result.Errors, e => e.ErrorMessage ==
            "Trebuie specificat cel putin un numar de telefon sau o adresa de email.");
    }

    [Fact]
    public void Invalid_WhenManyRequiredFieldsMissing_HasMultipleErrors()
    {
        var model = new ReaderCreateRequest
        {
            FirstName = "",
            LastName = "",
            Address = "",
            AccountId = "",
            PhoneNumber = null,
            Email = null
        };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
        result.ShouldHaveValidationErrorFor(x => x.Address);
        result.ShouldHaveValidationErrorFor(x => x.AccountId);

        Assert.Contains(result.Errors, e => e.ErrorMessage ==
            "Trebuie specificat cel putin un numar de telefon sau o adresa de email.");
    }
}