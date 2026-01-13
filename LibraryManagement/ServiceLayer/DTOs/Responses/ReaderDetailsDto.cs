namespace ServiceLayer.DTOs.Responses;

/// <summary>
/// Detalii despre profilul unui cititor.
/// </summary>
public class ReaderDetailsDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string ContactInfo { get; set; }
    public bool IsStaffAccount { get; set; }
    public int ActiveLoansCount { get; set; }
}