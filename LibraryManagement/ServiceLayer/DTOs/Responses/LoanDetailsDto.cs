namespace ServiceLayer.DTOs.Responses;

/// <summary>
/// Informatii despre un imprumut, folosite pentru afisarea in consola.
/// </summary>
public class LoanDetailsDto
{
    public int Id { get; set; }
    public string BookTitle { get; set; }
    public string ReaderName { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; }
}