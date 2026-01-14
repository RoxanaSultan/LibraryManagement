using FluentValidation.TestHelper;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.Validators;

namespace TestServiceLayer.Validators;

public class ExtensionRequestValidatorTests
{
    private readonly ExtensionRequestValidator _validator = new();

    [Fact]
    public void Valid_WhenLoanIdPositive_AndDaysBetween1And30()
    {
        var model = new ExtensionRequest { LoanId = 1, DaysRequested = 7 };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Invalid_WhenLoanIdNotPositive(int loanId)
    {
        var model = new ExtensionRequest { LoanId = loanId, DaysRequested = 5 };
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.LoanId)
            .WithErrorMessage("ID-ul imprumutului este invalid.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Invalid_WhenDaysRequestedLessThan1(int days)
    {
        var model = new ExtensionRequest { LoanId = 1, DaysRequested = days };
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.DaysRequested)
            .WithErrorMessage("Perioada de prelungire trebuie sa fie intre 1 si 30 de zile.");
    }

    [Theory]
    [InlineData(31)]
    [InlineData(60)]
    [InlineData(999)]
    public void Invalid_WhenDaysRequestedGreaterThan30(int days)
    {
        var model = new ExtensionRequest { LoanId = 1, DaysRequested = days };
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.DaysRequested)
            .WithErrorMessage("Perioada de prelungire trebuie sa fie intre 1 si 30 de zile.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(15)]
    public void Valid_WhenDaysRequestedIsInRange(int days)
    {
        var model = new ExtensionRequest { LoanId = 1, DaysRequested = days };
        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.DaysRequested);
    }

    [Fact]
    public void Invalid_WhenBothLoanIdAndDaysInvalid_HasTwoErrors()
    {
        var model = new ExtensionRequest { LoanId = 0, DaysRequested = 0 };
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.LoanId);
        result.ShouldHaveValidationErrorFor(x => x.DaysRequested);
    }
}