using DomainModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLibrary;

public static class DbInitializer
{
    public static async Task SeedData(LibraryDbContext context)
    {
        // 1. Seed Domains
        if (!await context.Domains.AnyAsync())
        {
            var science = new Domain { Name = "Science" };
            var computerScience = new Domain { Name = "Computer Science", ParentDomain = science };
            var databases = new Domain { Name = "Databases", ParentDomain = computerScience };

            await context.Domains.AddRangeAsync(science, computerScience, databases);
            await context.SaveChangesAsync();
        }

        // 2. Seed Authors
        if (!await context.Authors.AnyAsync())
        {
            await context.Authors.AddAsync(
                new Author { Name = "Andrew Tanenbaum" }
            );
            await context.SaveChangesAsync();
        }

        // 3. Seed Books and Editions
        if (!await context.Books.AnyAsync())
        {
            var csDomain = await context.Domains
                .FirstOrDefaultAsync(d => d.Name == "Computer Science");

            var author = await context.Authors
                .FirstOrDefaultAsync(a => a.Name == "Andrew Tanenbaum");

            if (csDomain != null && author != null)
            {
                var book = new Book
                {
                    Title = "Computer Networks",
                    Authors = new List<Author> { author },
                    ExplicitDomains = new List<Domain> { csDomain }
                };

                var edition = new Edition
                {
                    Book = book,
                    Publisher = "Pearson",
                    EditionNumber = "5th Edition",
                    InitialStock = 10,
                    CurrentStock = 10,
                    ReadingRoomOnlyCount = 2,
                    Year = 2021,
                    PageCount = 900,
                    BookType = "Hardcover"
                };

                await context.Books.AddAsync(book);
                await context.Editions.AddAsync(edition);
                await context.SaveChangesAsync();
            }
        }

        // 4. Seed Readers
        if (!await context.Readers.AnyAsync())
        {
            await context.Readers.AddRangeAsync(
                new Reader
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com",
                    Address = "Street 1",
                    AccountId = "ACC1",
                    PhoneNumber = "0700000001",
                    IsLibraryStaff = false
                },
                new Reader
                {
                    FirstName = "Jane",
                    LastName = "Staff",
                    Email = "jane@example.com",
                    Address = "Street 2",
                    AccountId = "ACC2",
                    PhoneNumber = "0700000002",
                    IsLibraryStaff = true
                }
            );

            await context.SaveChangesAsync();
        }
    }
}
