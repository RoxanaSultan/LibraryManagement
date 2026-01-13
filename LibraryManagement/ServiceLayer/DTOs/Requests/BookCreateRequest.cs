namespace ServiceLayer.DTOs.Requests;

/// <summary>
/// Obiect ce contine datele necesare pentru a crea o carte noua in sistem.
/// </summary>
public class BookCreateRequest
{
    /// <summary>
    /// Titlul cartii.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Lista de ID-uri ale domeniilor in care este incadrata explicit cartea.
    /// </summary>
    public List<int> DomainIds { get; set; } = new List<int>();

    /// <summary>
    /// Lista de ID-uri ale autorilor cartii.
    /// </summary>
    public List<int> AuthorIds { get; set; } = new List<int>();

    /// <summary>
    /// Datele pentru editia initiala (optional, poti adauga mai multe).
    /// </summary>
    public string Publisher { get; set; }
    public int Year { get; set; }
    public int PageCount { get; set; }
    public int InitialStock { get; set; }
}