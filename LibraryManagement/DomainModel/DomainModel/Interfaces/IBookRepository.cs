using DomainModel.Entities;

namespace DomainModel.Interfaces;

public interface IBookRepository
{
    Task<Book> GetByIdAsync(int id);
    Task<IEnumerable<Book>> GetAllAsync();
    Task AddAsync(Book book);
    Task UpdateAsync(Book book);

    // Metoda necesara pentru a verifica stocul unei editii specifice
    Task<Edition> GetEditionByIdAsync(int editionId);
}