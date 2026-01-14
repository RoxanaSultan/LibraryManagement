using System;
using System.Collections.Generic;
using System.Linq;
using DomainModel.Entities;

namespace DomainModel.Services;

public static partial class LoanValidator
{
    // Checks if the total extension days in the last 3 months for a loan do not exceed LIM
    public static bool CanExtendLoan(Loan loan, int limitDays)
    {
        var sinceDate = DateTime.Now.AddMonths(-3);

        // Sum only extensions granted in the last 3 months
        var totalExtensionDays = loan.Extensions
            .Where(e => e.ExtensionDate >= sinceDate)
            .Sum(e => e.DaysAdded);

        return totalExtensionDays < limitDays;
    }

    // Checks if the reader can borrow from a domain, considering past loans in the last L months
    public static bool CanBorrowFromDomain(
        int readerId,
        Domain targetDomain,
        IEnumerable<Loan> pastLoans,
        int maxPerDomain,
        int monthsWindow)
    {
        var sinceDate = DateTime.Now.AddMonths(-monthsWindow);

        // For each loan, get all domains of the book (including ancestors)
        var relevantLoans = pastLoans
            .Where(l => l.ReaderId == readerId && l.LoanDate >= sinceDate)
            .SelectMany(l => l.Edition?.Book?.GetAllDomains() ?? Enumerable.Empty<Domain>())
            .Where(domain => domain.Id == targetDomain.Id);

        int count = relevantLoans.Count();
        return count < maxPerDomain;
    }

    // C = max number of books per loan request
    public static bool IsValidLoanRequest(IEnumerable<Book> books, int maxBooks)
    {
        var bookList = books.ToList();
        if (bookList.Count == 0 || bookList.Count > maxBooks)
            return false;

        if (bookList.Count >= 3)
        {
            // Each book can have multiple domains, including ancestors
            var allCategories = new HashSet<Domain>();
            foreach (var book in bookList)
            {
                foreach (var domain in book.GetAllDomains())
                    allCategories.Add(domain);
            }
            if (allCategories.Count < 2)
                return false;
        }
        return true;
    }
}