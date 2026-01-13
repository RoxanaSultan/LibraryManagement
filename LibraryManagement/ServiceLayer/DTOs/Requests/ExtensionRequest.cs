namespace ServiceLayer.DTOs.Requests
{
    /// <summary>
    /// Datele necesare pentru a solicita prelungirea unui imprumut.
    /// </summary>
    public class ExtensionRequest
    {
        public int LoanId { get; set; }
        public int DaysRequested { get; set; }
    }
}