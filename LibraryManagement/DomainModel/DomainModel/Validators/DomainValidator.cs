using DomainModel.Entities;
using FluentValidation;

namespace DomainModel.Validators;

/// <summary>
/// Validator pentru entitatea Domain.
/// </summary>
public class DomainValidator : AbstractValidator<Domain>
{
    public DomainValidator()
    {
        RuleFor(d => d.Name)
            .NotEmpty().WithMessage("Numele domeniului este obligatoriu.")
            .MaximumLength(100);
    }
}