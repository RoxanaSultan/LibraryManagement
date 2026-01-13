namespace DomainModel.Exceptions;

/// <summary>
/// Exceptie aruncata cand nu exista suficiente exemplare disponibile (regula de 10%).
/// </summary>
public class InsufficientStockException : LibraryException
{
    public InsufficientStockException(string message) : base(message) { }
}