using System.Threading.Tasks;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.DTOs.Responses;

namespace ServiceLayer.Interfaces;

/// <summary>
/// Interfata principala pentru gestionarea procesului de imprumut.
/// </summary>
public interface ILoanService
{
    /// <summary>
    /// Realizeaza un imprumut nou, verificand toate constrangerile de business.
    /// </summary>
    /// <param name="request">Detalii despre cititor si editia ceruta.</param>
    /// <returns>Detalii despre imprumutul realizat.</returns>
    Task<LoanDetailsDto> BorrowBookAsync(BorrowRequest request);

    /// <summary>
    /// Prelungeste un imprumut existent, verificand limita LIM.
    /// </summary>
    /// <param name="request">ID-ul imprumutului si perioada.</param>
    Task ExtendLoanAsync(ExtensionRequest request);

    /// <summary>
    /// Inregistreaza returnarea unei carti.
    /// </summary>
    /// <param name="loanId">ID-ul tranzactiei de imprumut.</param>
    Task ReturnBookAsync(int loanId);
}