using DomainModel.Entities;
using DomainModel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLibrary.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly LibraryDbContext _context;

    public LoanRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Loan loan)
    {
        await _context.Loans.AddAsync(loan);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Loan loan)
    {
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync();
    }

    public async Task<Loan> GetByIdAsync(int id)
    {
        return await _context.Loans
            .Include(l => l.Edition)
            .Include(l => l.Extensions)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Loan>> GetLoansInPeriodAsync(int readerId, DateTime startDate, DateTime endDate)
    {
        return await _context.Loans
            .Where(l => l.ReaderId == readerId && l.LoanDate >= startDate && l.LoanDate <= endDate)
            .ToListAsync();
    }

    public async Task<Loan> GetLastLoanForBookAsync(int readerId, int bookId)
    {
        return await _context.Loans
            .Where(l => l.ReaderId == readerId && l.Edition.BookId == bookId)
            .OrderByDescending(l => l.LoanDate)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetTotalExtensionDaysInLastThreeMonthsAsync(int readerId)
    {
        var threeMonthsAgo = DateTime.Now.AddMonths(-3);
        return await _context.LoanExtensions
            .Where(le => le.Loan.ReaderId == readerId && le.ExtensionDate >= threeMonthsAgo)
            .SumAsync(le => le.DaysAdded);
    }

    public async Task<IEnumerable<Loan>> GetActiveLoansAsync(int readerId)
    {
        return await _context.Loans
            .Where(l => l.ReaderId == readerId && l.ReturnDate == null)
            .ToListAsync();
    }
}