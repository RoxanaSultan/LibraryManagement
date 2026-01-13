namespace DomainModel.Exceptions;

/// <summary>
/// Exceptie aruncata cand o carte incalca regulile ierarhiei de domenii.
/// </summary>
public class DomainHierarchyException : LibraryException
{
    public DomainHierarchyException(string message) : base(message) { }
}