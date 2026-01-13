using FluentValidation;
using ServiceLayer.DTOs.Requests;

namespace ServiceLayer.Validators;

/// <summary>
/// Validator pentru cererea de imprumut.
/// </summary>
public class BorrowRequestValidator : AbstractValidator<BorrowRequest>
{
    public BorrowRequestValidator()
    {
        RuleFor(x => x.ReaderId)
            .GreaterThan(0)
            .WithMessage("ID-ul cititorului trebuie sa fie un numar pozitiv.");

        RuleFor(x => x.EditionId)
            .GreaterThan(0)
            .WithMessage("ID-ul editiei trebuie sa fie un numar pozitiv.");
    }
}