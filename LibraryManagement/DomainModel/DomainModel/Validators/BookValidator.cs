namespace DomainModel.Validators;

using DomainModel.Entities;
using FluentValidation;

public class BookValidator : AbstractValidator<Book>
{
    // Trimitem valoarea maxima (citita din configurari) prin constructor
    public BookValidator(int maxDomainsLimit)
    {
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Titlul cartii este obligatoriu.");

        RuleFor(b => b.ExplicitDomains)
            .Must(domains => domains != null && domains.Count <= maxDomainsLimit)
            .WithMessage($"O carte nu poate face parte din mai mult de {maxDomainsLimit} domenii.");
    }
}