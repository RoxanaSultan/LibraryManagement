namespace ServiceLayer.DTOs.Responses;

/// <summary>
/// Rezumat al unei carti pentru afisarea in liste.
/// </summary>
public class BookListDto
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public List<string> Authors { get; set; }
    public List<string> Domains { get; set; }
    public int TotalAvailableCopies { get; set; }
    public bool IsAvailableForBorrowing { get; set; }
}