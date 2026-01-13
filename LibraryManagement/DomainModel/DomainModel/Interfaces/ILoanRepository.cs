using DomainModel.Entities;

namespace DomainModel.Interfaces;

public interface ILoanRepository
{
    Task AddAsync(Loan loan);
    Task UpdateAsync(Loan loan);
    Task<Loan> GetByIdAsync(int id);

    /// <summary>
    /// Returneaza toate imprumuturile active (nereturnate) ale unui cititor.
    /// </summary>
    Task<IEnumerable<Loan>> GetActiveLoansAsync(int readerId);

    /// <summary>
    /// Returneaza imprumuturile dintr-o perioada specifica (pentru NMC/PER).
    /// </summary>
    Task<IEnumerable<Loan>> GetLoansInPeriodAsync(int readerId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Returneaza ultimul imprumut al unei carti specifice de catre un cititor (pentru DELTA).
    /// </summary>
    Task<Loan> GetLastLoanForBookAsync(int readerId, int bookId);

    /// <summary>
    /// Calculeaza suma prelungirilor in ultimele 3 luni pentru un cititor (pentru LIM).
    /// </summary>
    Task<int> GetTotalExtensionDaysInLastThreeMonthsAsync(int readerId);
}