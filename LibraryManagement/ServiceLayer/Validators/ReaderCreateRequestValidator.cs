using FluentValidation;
using ServiceLayer.DTOs.Requests;

namespace ServiceLayer.Validators;

public class ReaderCreateRequestValidator : AbstractValidator<ReaderCreateRequest>
{
    public ReaderCreateRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Prenumele este obligatoriu.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Numele este obligatoriu.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adresa este obligatorie.");

        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("ID-ul contului este obligatoriu.");

        // Macar unul: telefon sau email
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.PhoneNumber) || !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Trebuie specificat cel putin un numar de telefon sau o adresa de email.");

        // Email valid (daca e completat) - mai strict decat EmailAddress()
        RuleFor(x => x.Email)
            .Matches(@"^[^@\s]+@[^@\s\.](?:[^@\s]*[^@\s\.])?\.[^@\s]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Adresa de email nu are un format valid.");
    }
}