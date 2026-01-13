using DomainModel.Entities;
using DomainModel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLibrary.Repositories;

/// <summary>
/// Repository pentru gestionarea domeniilor si a ierarhiei acestora.
/// </summary>
public class DomainRepository : IDomainRepository
{
    private readonly LibraryDbContext _context;

    public DomainRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Domain> GetByIdAsync(int id)
    {
        return await _context.Domains
            .Include(d => d.ParentDomain)
            .Include(d => d.SubDomains)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Domain>> GetAllAsync()
    {
        return await _context.Domains.ToListAsync();
    }

    /// <summary>
    /// Recupereaza toti stramosii unui domeniu (parinte, bunic, etc.).
    /// </summary>
    /// <param name="domainId">ID-ul domeniului de plecare.</param>
    /// <returns>O lista cu toti stramosii gasiti.</returns>
    public async Task<IEnumerable<Domain>> GetAncestorsAsync(int domainId)
    {
        var ancestors = new List<Domain>();
        var current = await _context.Domains.FindAsync(domainId);

        while (current != null && current.ParentDomainId.HasValue)
        {
            current = await _context.Domains
                .FirstOrDefaultAsync(d => d.Id == current.ParentDomainId.Value);

            if (current != null)
            {
                ancestors.Add(current);
            }
        }

        return ancestors;
    }
}