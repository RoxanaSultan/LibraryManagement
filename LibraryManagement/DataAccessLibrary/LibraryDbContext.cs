using DomainModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLibrary;

/// <summary>
/// Contextul bazei de date pentru sistemul de gestiune al bibliotecii.
/// Configureaza maparea entitatilor catre tabelele SQL Server.
/// </summary>
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    // Tabelele bazei de date
    public DbSet<Book> Books { get; set; }
    public DbSet<Domain> Domains { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Reader> Readers { get; set; }
    public DbSet<Edition> Editions { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<LoanExtension> LoanExtensions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configurare Ierarhie Domenii (Self-Referencing)
        modelBuilder.Entity<Domain>()
            .HasOne(d => d.ParentDomain)
            .WithMany(d => d.SubDomains)
            .HasForeignKey(d => d.ParentDomainId)
            .OnDelete(DeleteBehavior.Restrict); // Prevenim stergerea in cascada pentru a evita ciclurile

        // 2. Relație Many-to-Many: Book <-> Domain (Domeniile explicite ale unei carti)
        modelBuilder.Entity<Book>()
            .HasMany(b => b.ExplicitDomains)
            .WithMany(d => d.Books)
            .UsingEntity(j => j.ToTable("BookExplicitDomains"));

        // 3. Relație Many-to-Many: Book <-> Author
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Authors)
            .WithMany(a => a.Books)
            .UsingEntity(j => j.ToTable("BookAuthors"));

        // 4. Relație One-to-Many: Book <-> Edition
        modelBuilder.Entity<Edition>()
            .HasOne(e => e.Book)
            .WithMany(b => b.Editions)
            .HasForeignKey(e => e.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // 5. Configurare Imprumuturi (Loans)
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Reader)
            .WithMany() // Nu am adaugat ICollection<Loan> in Reader pentru a ramane curat, dar se poate adauga
            .HasForeignKey(l => l.ReaderId);

        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Edition)
            .WithMany()
            .HasForeignKey(l => l.EditionId);

        // 6. Relație One-to-Many: Loan <-> LoanExtension
        modelBuilder.Entity<LoanExtension>()
            .HasOne(le => le.Loan)
            .WithMany(l => l.Extensions)
            .HasForeignKey(le => le.LoanId)
            .OnDelete(DeleteBehavior.Cascade);

        // 7. Constrangeri suplimentare pentru Reader (Validarile de baza la nivel de DB)
        modelBuilder.Entity<Reader>()
            .HasIndex(r => r.AccountId); // Index pentru cautarea rapida a contului partajat
    }
}