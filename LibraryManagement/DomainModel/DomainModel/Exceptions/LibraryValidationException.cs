using System.Collections.Generic;
using System.Linq;

namespace DomainModel.Exceptions;

/// <summary>
/// Exceptie aruncata cand datele unei entitati nu trec de validatoarele FluentValidation.
/// </summary>
public class LibraryValidationException : LibraryException
{
    public IEnumerable<string> Errors { get; }

    public LibraryValidationException(IEnumerable<string> errors)
        : base("Eroare de validare a datelor.")
    {
        Errors = errors;
    }

    public override string ToString()
    {
        return $"{base.Message} Detalii: {string.Join(", ", Errors)}";
    }
}