using FluentValidation.TestHelper;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.Validators;
using Xunit;

namespace TestServiceLayer.Validators;

public class BorrowRequestValidatorTests
{
    private readonly BorrowRequestValidator _validator = new();

    [Fact]
    public void Valid_WhenIdsArePositive()
    {
        var model = new BorrowRequest { ReaderId = 1, EditionId = 2 };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Invalid_WhenReaderIdNotPositive(int readerId)
    {
        var model = new BorrowRequest { ReaderId = readerId, EditionId = 1 };
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ReaderId)
            .WithErrorMessage("ID-ul cititorului trebuie sa fie un numar pozitiv.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Invalid_WhenEditionIdNotPositive(int editionId)
    {
        var model = new BorrowRequest { ReaderId = 1, EditionId = editionId };
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.EditionId)
            .WithErrorMessage("ID-ul editiei trebuie sa fie un numar pozitiv.");
    }

    [Fact]
    public void Invalid_WhenBothIdsNotPositive_HasTwoErrors()
    {
        var model = new BorrowRequest { ReaderId = 0, EditionId = 0 };
        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ReaderId);
        result.ShouldHaveValidationErrorFor(x => x.EditionId);
    }
}