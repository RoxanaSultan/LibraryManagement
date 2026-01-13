using DomainModel.Entities;
using DomainModel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLibrary.Repositories;

public class ReaderRepository : IReaderRepository
{
    private readonly LibraryDbContext _context;

    public ReaderRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Reader> GetByIdAsync(int id)
    {
        return await _context.Readers.FindAsync(id);
    }

    public async Task<IEnumerable<Reader>> GetReadersByAccountIdAsync(string accountId)
    {
        return await _context.Readers
            .Where(r => r.AccountId == accountId)
            .ToListAsync();
    }

    public async Task AddAsync(Reader reader)
    {
        await _context.Readers.AddAsync(reader);
        await _context.SaveChangesAsync();
    }
}