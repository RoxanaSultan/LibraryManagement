using DataAccessLibrary;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.Settings;
using DomainModel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.Interfaces;
using ServiceLayer.Mappings;
using ServiceLayer.Services;

namespace MyApplication;

/// <summary>
/// Entry point for the Library Management System.
/// </summary>
internal class Program
{
    /// <summary>
    /// Main entry method.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private static async Task Main(string[] args)
    {
        // 1. Load Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. Setup Dependency Injection
        var services = new ServiceCollection();
        ConfigureServices(services, configuration);

        var serviceProvider = services.BuildServiceProvider();

        // 3. Database Initialization
        try
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerService>();

                Console.WriteLine("System: Initializing database...");

                // Uncomment the next line only ONCE if you need to wipe the DB and start fresh
                // await context.Database.EnsureDeletedAsync(); 

                await context.Database.MigrateAsync();
                await DbInitializer.SeedData(context);

                logger.LogInformation("System started successfully.");
                Console.WriteLine("System: Ready.");
            }

            // 4. Start the Application Menu
            await RunUserInterface(serviceProvider);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Details: {ex.InnerException.Message}");
        }
    }

    /// <summary>
    /// Configures the DI container.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<LibraryDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Settings & Infrastructure
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ILibrarySettings, DatabaseLibrarySettings>();
        services.AddSingleton<ILoggerService, LibraryLogger>();

        // Repositories (Data Layer)
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IDomainRepository, DomainRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IReaderRepository, ReaderRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();

        // Services (Service Layer)
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IReaderService, ReaderService>();

        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));
    }

    /// <summary>
    /// Simple console menu to interact with the library system.
    /// </summary>
    private static async Task RunUserInterface(IServiceProvider sp)
    {
        var loanService = sp.GetRequiredService<ILoanService>();
        var bookService = sp.GetRequiredService<IBookService>();
        var logger = sp.GetRequiredService<ILoggerService>();

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("    LIBRARY MANAGEMENT SYSTEM (v1.0)");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Borrow a Book");
            Console.WriteLine("2. Return a Book");
            Console.WriteLine("3. List All Books");
            Console.WriteLine("4. Exit");
            Console.Write("Select an option: ");

            var input = Console.ReadLine();
            try
            {
                switch (input)
                {
                    case "1":
                        Console.Write("Enter Reader ID: ");
                        int rId = int.Parse(Console.ReadLine());
                        Console.Write("Enter Edition ID: ");
                        int eId = int.Parse(Console.ReadLine());

                        var loanResult = await loanService.BorrowBookAsync(new BorrowRequest
                        {
                            ReaderId = rId,
                            EditionId = eId
                        });
                        Console.WriteLine($"SUCCESS: Due date is {loanResult.DueDate:yyyy-MM-dd}");
                        break;

                    case "2":
                        Console.Write("Enter Loan ID to return: ");
                        int loanId = int.Parse(Console.ReadLine());
                        await loanService.ReturnBookAsync(loanId);
                        Console.WriteLine("SUCCESS: Book returned.");
                        break;

                    case "3":
                        var books = await bookService.GetAllBooksAsync();
                        Console.WriteLine("\nAVAILABLE BOOKS:");
                        foreach (var b in books)
                        {
                            Console.WriteLine($"- [{b.BookId}] {b.Title} (Stock: {b.TotalAvailableCopies})");
                        }
                        break;

                    case "4":
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"User action failed: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}