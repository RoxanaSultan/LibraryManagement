namespace ServiceLayer.DTOs.Requests;

/// <summary>
/// Datele necesare pentru a solicita un imprumut.
/// </summary>
public class BorrowRequest
{
    public int ReaderId { get; set; }
    public int EditionId { get; set; }
}