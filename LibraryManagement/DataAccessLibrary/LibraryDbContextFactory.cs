using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccessLibrary;

/// <summary>
/// Fabrica folosita de Entity Framework Core Tools pentru a genera migrarile la design-time.
/// Aceasta clasa este necesara deoarece DbContext-ul se afla intr-o librarie separata de proiectul de startup.
/// </summary>
public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    public LibraryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();

        // Pune aici ACELASI connection string pe care l-ai pus in appsettings.json
        // Este folosit doar pentru generarea structurii bazei de date
        optionsBuilder.UseSqlServer("Server=DESKTOP-DD2F7GM\\MSSQLSERVER01;Database=LibraryDb;Trusted_Connection=True;TrustServerCertificate=True;");

        return new LibraryDbContext(optionsBuilder.Options);
    }
}