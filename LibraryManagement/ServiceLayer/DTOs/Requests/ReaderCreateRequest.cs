namespace ServiceLayer.DTOs.Requests;

/// <summary>
/// Datele necesare pentru crearea unui cont de cititor.
/// </summary>
public class ReaderCreateRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public bool IsStaff { get; set; }
    public string AccountId { get; set; }
}