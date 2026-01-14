using DomainModel.Entities;
using FluentValidation;

namespace DomainModel.Validators;

public class ReaderValidator : AbstractValidator<Reader>
{
    public ReaderValidator()
    {
        RuleFor(r => r.FirstName)
            .NotEmpty().WithMessage("Prenumele este obligatoriu.")
            .MaximumLength(50);

        RuleFor(r => r.LastName)
            .NotEmpty().WithMessage("Numele este obligatoriu.")
            .MaximumLength(50);

        RuleFor(r => r.Address)
            .NotEmpty().WithMessage("Adresa este obligatorie.");

        RuleFor(r => r)
            .Must(r => !string.IsNullOrWhiteSpace(r.PhoneNumber) || !string.IsNullOrWhiteSpace(r.Email))
            .WithMessage("Trebuie sa specificati cel putin o modalitate de contact (Telefon sau Email).");

        RuleFor(r => r.Email)
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            .When(r => !string.IsNullOrWhiteSpace(r.Email))
            .WithMessage("Formatul adresei de email este invalid.");
    }
}