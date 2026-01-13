using DomainModel.Entities;
using FluentValidation;

namespace DomainModel.Validators;

/// <summary>
/// Validator pentru entitatea Author.
/// </summary>
public class AuthorValidator : AbstractValidator<Author>
{
    public AuthorValidator()
    {
        RuleFor(a => a.Name)
            .NotEmpty().WithMessage("Numele autorului este obligatoriu.")
            .MaximumLength(150);
    }
}