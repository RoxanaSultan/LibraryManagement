namespace DomainModel.Validators;

using DomainModel.Entities;
using FluentValidation;

public class BookValidator : AbstractValidator<Book>
{
    public BookValidator(int maxDomainsLimit)
    {
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Titlul cartii este obligatoriu.");

        RuleFor(b => b.ExplicitDomains)
            .NotEmpty()
            .WithMessage("Cartea trebuie sa apartina de cel putin un domeniu.");

        RuleFor(b => b.ExplicitDomains)
            .Must(domains => domains != null && domains.Count <= maxDomainsLimit)
            .WithMessage($"O carte nu poate face parte din mai mult de {maxDomainsLimit} domenii.");
    }
}