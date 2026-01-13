namespace DomainModel.Exceptions;

/// <summary>
/// Exceptie aruncata cand un cititor incalca limitele de imprumut (pragurile setate).
/// </summary>
public class LoanRuleViolationException : LibraryException
{
    public string RuleName { get; } // Ex: "NMC", "DELTA", "NCZ"

    public LoanRuleViolationException(string ruleName, string message)
        : base(message)
    {
        RuleName = ruleName;
    }
}