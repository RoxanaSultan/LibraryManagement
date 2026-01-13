using DomainModel.Entities;

namespace DomainModel.Interfaces;

public interface IReaderRepository
{
    Task<Reader> GetByIdAsync(int id);
    Task<IEnumerable<Reader>> GetReadersByAccountIdAsync(string accountId);
    Task AddAsync(Reader reader);
}