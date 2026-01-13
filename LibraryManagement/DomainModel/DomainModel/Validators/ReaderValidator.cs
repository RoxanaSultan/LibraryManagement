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

        // Regula speciala: Macar telefon SAU email
        RuleFor(r => r)
            .Must(r => !string.IsNullOrEmpty(r.PhoneNumber) || !string.IsNullOrEmpty(r.Email))
            .WithMessage("Trebuie sa specificati cel putin o modalitate de contact (Telefon sau Email).");

        // Validare format email (daca este completat)
        RuleFor(r => r.Email)
            .EmailAddress().When(r => !string.IsNullOrEmpty(r.Email))
            .WithMessage("Formatul adresei de email este invalid.");
    }
}