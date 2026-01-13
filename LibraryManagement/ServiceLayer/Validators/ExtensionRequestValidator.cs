using FluentValidation;
using ServiceLayer.DTOs.Requests;

namespace ServiceLayer.Validators;

/// <summary>
/// Validator pentru cererea de prelungire a unui imprumut.
/// </summary>
public class ExtensionRequestValidator : AbstractValidator<ExtensionRequest>
{
    public ExtensionRequestValidator()
    {
        RuleFor(x => x.LoanId)
            .GreaterThan(0)
            .WithMessage("ID-ul imprumutului este invalid.");

        RuleFor(x => x.DaysRequested)
            .InclusiveBetween(1, 30)
            .WithMessage("Perioada de prelungire trebuie sa fie intre 1 si 30 de zile.");
    }
}