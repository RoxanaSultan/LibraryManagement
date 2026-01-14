using DomainModel.Entities;
using DomainModel.Services;
using Xunit;
using System;
using System.Collections.Generic;

public class LoanValidatorDomainLimitTests
{
    [Fact]
    public void CanBorrowFromDomain_RespectsDomainLimit()
    {
        var domain = new Domain { Id = 1, Name = "Informatica" };
        var book = new Book { ExplicitDomains = new List<Domain> { domain } };
        var edition = new Edition { Book = book };

        var readerId = 42;
        var now = DateTime.Now;

        var pastLoans = new List<Loan>
        {
            new Loan { ReaderId = readerId, LoanDate = now.AddDays(-10), Edition = edition },
            new Loan { ReaderId = readerId, LoanDate = now.AddDays(-20), Edition = edition }
        };

        // D = 2, L = 1 (month)
        Assert.False(LoanValidator.CanBorrowFromDomain(readerId, domain, pastLoans, 2, 1));
        Assert.True(LoanValidator.CanBorrowFromDomain(readerId, domain, pastLoans, 3, 1));
    }

    [Fact]
    public void CanBorrowFromDomain_IgnoresLoansOutsideWindow()
    {
        var domain = new Domain { Id = 1, Name = "Informatica" };
        var book = new Book { ExplicitDomains = new List<Domain> { domain } };
        var edition = new Edition { Book = book };

        var readerId = 42;
        var now = DateTime.Now;

        var pastLoans = new List<Loan>
        {
            new Loan { ReaderId = readerId, LoanDate = now.AddMonths(-2), Edition = edition }
        };

        // D = 1, L = 1 (month)
        Assert.True(LoanValidator.CanBorrowFromDomain(readerId, domain, pastLoans, 1, 1));
    }
}

public class LoanValidatorTests
{
    [Fact]
    public void IsValidLoanRequest_AllowsUpToMaxBooks()
    {
        var books = new List<Book>
        {
            new Book { ExplicitDomains = new List<Domain> { new Domain { Id = 1 } } },
            new Book { ExplicitDomains = new List<Domain> { new Domain { Id = 2 } } }
        };
        Assert.True(LoanValidator.IsValidLoanRequest(books, 2));
        Assert.False(LoanValidator.IsValidLoanRequest(books, 1));
    }

    [Fact]
    public void IsValidLoanRequest_RequiresAtLeastTwoCategoriesForThreeOrMoreBooks()
    {
        var cat1 = new Domain { Id = 1, Name = "Cat1" };
        var cat2 = new Domain { Id = 2, Name = "Cat2" };

        var booksSameCat = new List<Book>
        {
            new Book { ExplicitDomains = new List<Domain> { cat1 } },
            new Book { ExplicitDomains = new List<Domain> { cat1 } },
            new Book { ExplicitDomains = new List<Domain> { cat1 } }
        };
        Assert.False(LoanValidator.IsValidLoanRequest(booksSameCat, 5));

        var booksDiffCat = new List<Book>
        {
            new Book { ExplicitDomains = new List<Domain> { cat1 } },
            new Book { ExplicitDomains = new List<Domain> { cat2 } },
            new Book { ExplicitDomains = new List<Domain> { cat1 } }
        };
        Assert.True(LoanValidator.IsValidLoanRequest(booksDiffCat, 5));
    }
}