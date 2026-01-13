namespace DomainModel.Exceptions;

/// <summary>
/// Exceptie aruncata cand o carte depaseste numarul maxim de domenii permise (pragul DOMENII).
/// </summary>
public class DomainConstraintException : LibraryException
{
    public DomainConstraintException(string message) : base(message) { }
}