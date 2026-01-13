namespace DomainModel.Exceptions;

/// <summary>
/// Exceptia de baza pentru toate erorile de business din aplicatia biblioteca.
/// </summary>
public class LibraryException : Exception
{
    public LibraryException(string message) : base(message) { }
    public LibraryException(string message, Exception innerException) : base(message, innerException) { }
}