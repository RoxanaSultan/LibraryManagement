using DomainModel.Entities;

namespace DomainModel.Interfaces;

/// <summary>
/// Defineste contractul pentru operatiile de persistenta ale autorilor.
/// </summary>
public interface IAuthorRepository
{
    /// <summary>
    /// Recupereaza un autor dupa identificatorul unic.
    /// </summary>
    /// <param name="id">ID-ul autorului.</param>
    /// <returns>Obiectul Author sau null daca nu exista.</returns>
    Task<Author> GetByIdAsync(int id);

    /// <summary>
    /// Returneaza toti autorii din baza de date.
    /// </summary>
    /// <returns>O colectie de autori.</returns>
    Task<IEnumerable<Author>> GetAllAsync();

    /// <summary>
    /// Adauga un autor nou in sistem.
    /// </summary>
    /// <param name="author">Entitatea autor de salvat.</param>
    Task AddAsync(Author author);

    /// <summary>
    /// Actualizeaza datele unui autor existent.
    /// </summary>
    /// <param name="author">Entitatea cu datele actualizate.</param>
    Task UpdateAsync(Author author);
}