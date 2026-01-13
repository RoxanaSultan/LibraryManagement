using System.Threading.Tasks;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.DTOs.Responses;

namespace ServiceLayer.Interfaces;

/// <summary>
/// Defineste operatiile administrative pentru cititorii bibliotecii.
/// </summary>
public interface IReaderService
{
    /// <summary>
    /// Inregistreaza un nou cititor in sistem.
    /// </summary>
    /// <param name="request">Datele cititorului.</param>
    /// <returns>Detaliile cititorului creat.</returns>
    Task<ReaderDetailsDto> RegisterReaderAsync(ReaderCreateRequest request);

    /// <summary>
    /// Obtine detaliile unui cititor dupa ID.
    /// </summary>
    /// <param name="readerId">ID-ul cititorului.</param>
    /// <returns>DTO cu datele cititorului.</returns>
    Task<ReaderDetailsDto> GetReaderByIdAsync(int readerId);
}