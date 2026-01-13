using FluentValidation;
using ServiceLayer.DTOs.Requests;

namespace ServiceLayer.Validators;

/// <summary>
/// Validator pentru crearea unui cititor nou.
/// </summary>
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

        // Regula Pagina 1: Macar unul din: numar de telefon, adresa de email
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.PhoneNumber) || !string.IsNullOrEmpty(x.Email))
            .WithMessage("Trebuie specificat cel putin un numar de telefon sau o adresa de email.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Adresa de email nu are un format valid.");
    }
}