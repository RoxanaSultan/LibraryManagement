using AutoMapper;
using DomainModel.Entities;
using DomainModel.Exceptions;
using DomainModel.Interfaces;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.DTOs.Responses;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

/// <summary>
/// Implementeaza logica complexa de business pentru imprumuturi si returnari.
/// </summary>
public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IReaderRepository _readerRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILibrarySettings _settings;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public LoanService(
        ILoanRepository loanRepository,
        IReaderRepository readerRepository,
        IBookRepository bookRepository,
        ILibrarySettings settings,
        IMapper mapper,
        ILoggerService logger)
    {
        _loanRepository = loanRepository;
        _readerRepository = readerRepository;
        _bookRepository = bookRepository;
        _settings = settings;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LoanDetailsDto> BorrowBookAsync(BorrowRequest request)
    {
        _logger.LogInformation($"Procesare cerere imprumut: Cititor {request.ReaderId}, Editia {request.EditionId}");

        // 1. Validare Existenta Entitati
        var reader = await _readerRepository.GetByIdAsync(request.ReaderId)
            ?? throw new LibraryException("Cititorul nu exista.");
        var edition = await _bookRepository.GetEditionByIdAsync(request.EditionId)
            ?? throw new LibraryException("Editia nu exista.");

        // 2. Calcul Praguri (Regula Personal Biblioteca - Pagina 2)
        // Daca e staff: NMC, C, D, LIM se dubleaza; DELTA, PER se injumatatesc.
        double multiplier = reader.IsLibraryStaff ? 2.0 : 1.0;
        double timeReducer = reader.IsLibraryStaff ? 0.5 : 1.0;

        int nmcLimit = (int)(_settings.NMC * multiplier);
        int perDays = (int)(_settings.PER * timeReducer);
        int nczLimit = reader.IsLibraryStaff ? _settings.PERSIMP : _settings.NCZ;
        int deltaDays = (int)(_settings.DELTA * timeReducer);

        // 3. Regula 10% Stoc (Pagina 1)
        int availableNonReadingRoom = edition.CurrentStock - edition.ReadingRoomOnlyCount;
        double tenPercentInitial = edition.InitialStock * 0.1;

        if (availableNonReadingRoom <= tenPercentInitial)
        {
            _logger.LogWarning("Incalcare regula 10% stoc.");
            throw new InsufficientStockException("Nu se mai pot face imprumuturi. Trebuie sa ramana minim 10% din fondul initial.");
        }

        // 4. Regula NCZ / PERSIMP (Limita zilnica)
        var loansToday = await _loanRepository.GetLoansInPeriodAsync(reader.Id, DateTime.Today, DateTime.Now);
        if (loansToday.Count() >= nczLimit)
        {
            throw new LoanRuleViolationException("NCZ", "Ati atins limita maxima de carti pe zi.");
        }

        // 5. Regula NMC / PER (Limita pe perioada)
        var startDatePer = DateTime.Now.AddDays(-perDays);
        var loansInPer = await _loanRepository.GetLoansInPeriodAsync(reader.Id, startDatePer, DateTime.Now);
        if (loansInPer.Count() >= nmcLimit)
        {
            throw new LoanRuleViolationException("NMC", $"Ati atins limita de {nmcLimit} carti in ultimele {perDays} zile.");
        }

        // 6. Regula DELTA (Aceeasi carte - Pagina 1)
        var lastLoan = await _loanRepository.GetLastLoanForBookAsync(reader.Id, edition.BookId);
        if (lastLoan != null && (DateTime.Now - lastLoan.LoanDate).TotalDays < deltaDays)
        {
            throw new LoanRuleViolationException("DELTA", $"Nu puteti re-imprumuta aceeasi carte inainte de {deltaDays} zile.");
        }

        // 7. Executie Imprumut
        var loan = new Loan
        {
            ReaderId = reader.Id,
            EditionId = edition.Id,
            LoanDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(14) // Termen standard 2 saptamani
        };

        // Actualizam stocul
        edition.CurrentStock--;

        await _loanRepository.AddAsync(loan);
        _logger.LogInformation("Imprumut realizat cu succes.");

        return _mapper.Map<LoanDetailsDto>(loan);
    }

    public async Task ExtendLoanAsync(ExtensionRequest request)
    {
        var loan = await _loanRepository.GetByIdAsync(request.LoanId)
            ?? throw new LibraryException("Imprumutul nu exista.");

        var reader = await _readerRepository.GetByIdAsync(loan.ReaderId);

        // Calcul prag LIM (Personal biblioteca)
        int limLimit = (int)(_settings.LIM * (reader.IsLibraryStaff ? 2.0 : 1.0));

        // Regula LIM (Suma prelungirilor in ultimele 3 luni)
        int totalDays = await _loanRepository.GetTotalExtensionDaysInLastThreeMonthsAsync(reader.Id);

        if (totalDays + request.DaysRequested > limLimit)
        {
            throw new LoanRuleViolationException("LIM", "Suma prelungirilor depaseste limita admisa pe ultimele 3 luni.");
        }

        var extension = new LoanExtension
        {
            LoanId = loan.Id,
            ExtensionDate = DateTime.Now,
            DaysAdded = request.DaysRequested
        };

        loan.DueDate = loan.DueDate.AddDays(request.DaysRequested);

        await _loanRepository.UpdateAsync(loan);
        _logger.LogInformation($"Imprumut {loan.Id} prelungit cu {request.DaysRequested} zile.");
    }

    public async Task ReturnBookAsync(int loanId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId)
            ?? throw new LibraryException("Imprumutul nu exista.");

        if (loan.ReturnDate.HasValue)
            throw new LibraryException("Cartea a fost deja returnata.");

        loan.ReturnDate = DateTime.Now;
        loan.Edition.CurrentStock++;

        await _loanRepository.UpdateAsync(loan);
        _logger.LogInformation($"Cartea din imprumutul {loanId} a fost returnata.");
    }
}