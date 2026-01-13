using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.DTOs.Responses;

namespace ServiceLayer.Interfaces;

/// <summary>
/// Interfata pentru gestionarea colectiei de carti si domenii.
/// </summary>
public interface IBookService
{
    /// <summary>
    /// Adauga o carte noua si editiile sale, verificand ierarhia de domenii.
    /// </summary>
    /// <param name="request">Datele cartii si editiile.</param>
    Task AddBookAsync(BookCreateRequest request);

    /// <summary>
    /// Returneaza toate cartile disponibile pentru afisare.
    /// </summary>
    Task<IEnumerable<BookListDto>> GetAllBooksAsync();

    /// <summary>
    /// Cauta carti intr-un domeniu specific (incluzand subdomenii).
    /// </summary>
    /// <param name="domainId">ID-ul domeniului.</param>
    Task<IEnumerable<BookListDto>> GetBooksByDomainAsync(int domainId);
}