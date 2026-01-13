using DomainModel.Entities;
using DomainModel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLibrary.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Book> GetByIdAsync(int id)
    {
        return await _context.Books
            .Include(b => b.ExplicitDomains)
            .Include(b => b.Authors)
            .Include(b => b.Editions)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        return await _context.Books
            .Include(b => b.ExplicitDomains)
            .Include(b => b.Authors)
            .ToListAsync();
    }

    public async Task AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Book book)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync();
    }

    public async Task<Edition> GetEditionByIdAsync(int editionId)
    {
        return await _context.Editions
            .Include(e => e.Book)
            .FirstOrDefaultAsync(e => e.Id == editionId);
    }
}