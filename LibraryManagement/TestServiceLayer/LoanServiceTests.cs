using AutoMapper;
using DomainModel.Entities;
using DomainModel.Exceptions;
using DomainModel.Interfaces;
using Moq;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.Interfaces;
using ServiceLayer.Services;

namespace TestServiceLayer;

public class LoanServiceTests
{
    private readonly Mock<ILoanRepository> _loanRepo = new();
    private readonly Mock<IReaderRepository> _readerRepo = new();
    private readonly Mock<IBookRepository> _bookRepo = new();
    private readonly Mock<ILibrarySettings> _settings = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ILoggerService> _logger = new();

    private LoanService CreateSut()
        => new LoanService(_loanRepo.Object, _readerRepo.Object, _bookRepo.Object, _settings.Object, _mapper.Object, _logger.Object);

    private void SetupDefaultSettings()
    {
        _settings.SetupGet(s => s.NMC).Returns(5);
        _settings.SetupGet(s => s.PER).Returns(30);
        _settings.SetupGet(s => s.NCZ).Returns(2);
        _settings.SetupGet(s => s.PERSIMP).Returns(10);
        _settings.SetupGet(s => s.DELTA).Returns(14);
        _settings.SetupGet(s => s.LIM).Returns(20);
    }

    private static Reader Reader(int id = 1, bool staff = false)
        => new Reader { Id = id, FirstName = "A", LastName = "B", IsLibraryStaff = staff, AccountId = "acc" };

    private static Edition Edition(int id = 1, int bookId = 10, int initial = 100, int current = 80, int readingOnly = 0)
        => new Edition
        {
            Id = id,
            BookId = bookId,
            Publisher = "P",
            Year = 2020,
            InitialStock = initial,
            CurrentStock = current,
            ReadingRoomOnlyCount = readingOnly
        };

    [Fact]
    public async Task BorrowBookAsync_WhenReaderMissing_ThrowsLibraryException()
    {
        SetupDefaultSettings();
        _readerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Reader)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = 1, EditionId = 1 }));
    }

    [Fact]
    public async Task BorrowBookAsync_WhenEditionMissing_ThrowsLibraryException()
    {
        SetupDefaultSettings();
        _readerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Reader());
        _bookRepo.Setup(r => r.GetEditionByIdAsync(1)).ReturnsAsync((Edition)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = 1, EditionId = 1 }));
    }

    [Fact]
    public async Task BorrowBookAsync_WhenLoansTodayAtNCZ_Throws()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var edition = Edition(current: 80);

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan> { new Loan(), new Loan() }); // NCZ=2

        var sut = CreateSut();

        await Assert.ThrowsAsync<LoanRuleViolationException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id }));
    }

    [Fact]
    public async Task BorrowBookAsync_WhenStaffLoansTodayAtPERSIMP_Throws()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: true);
        var edition = Edition(current: 80);

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(Enumerable.Range(0, 10).Select(_ => new Loan()).ToList()); // PERSIMP=10

        var sut = CreateSut();

        await Assert.ThrowsAsync<LoanRuleViolationException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id }));
    }

    [Fact]
    public async Task BorrowBookAsync_WhenLoansInPERAtNMC_Throws()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var edition = Edition(current: 80);

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        // azi sub NCZ
        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan>());

        // in PER: NMC=5
        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, It.Is<DateTime>(d => d <= DateTime.Now.AddDays(-1)), It.IsAny<DateTime>()))
            .ReturnsAsync(Enumerable.Range(0, 5).Select(_ => new Loan()).ToList());

        var sut = CreateSut();

        await Assert.ThrowsAsync<LoanRuleViolationException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id }));
    }

    [Fact]
    public async Task BorrowBookAsync_WhenDeltaNotPassed_Throws()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var edition = Edition(bookId: 99, current: 80);

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan>());

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan>());

        _loanRepo.Setup(l => l.GetLastLoanForBookAsync(reader.Id, edition.BookId))
            .ReturnsAsync(new Loan { LoanDate = DateTime.Now.AddDays(-3) }); // DELTA=14

        var sut = CreateSut();

        await Assert.ThrowsAsync<LoanRuleViolationException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id }));
    }

    [Fact]
    public async Task BorrowBookAsync_WhenStockBelow10Percent_ThrowsInsufficientStockException()
    {
        SetupDefaultSettings();

        var reader = Reader();
        // initial=100, readingOnly=11, current=20 => availableNonReadingRoom=9, 10% = 10
        var edition = Edition(initial: 100, current: 20, readingOnly: 11);

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan>());

        _loanRepo.Setup(l => l.GetLastLoanForBookAsync(reader.Id, edition.BookId))
            .ReturnsAsync((Loan)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<InsufficientStockException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id }));
    }

    [Fact]
    public async Task BorrowBookAsync_WhenAllRulesPass_AddsLoan_DecrementsStock_ReturnsDto()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var edition = Edition(initial: 100, current: 80, readingOnly: 0);

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan>());

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan>());

        _loanRepo.Setup(l => l.GetLastLoanForBookAsync(reader.Id, edition.BookId))
            .ReturnsAsync((Loan)null!);

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.LoanDetailsDto>(It.IsAny<Loan>()))
            .Returns(new ServiceLayer.DTOs.Responses.LoanDetailsDto { Id = 1, DueDate = DateTime.Now.AddDays(14) });

        var sut = CreateSut();

        var dto = await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        _loanRepo.Verify(r => r.AddAsync(It.Is<Loan>(l => l.ReaderId == reader.Id && l.EditionId == edition.Id)), Times.Once);
        Assert.Equal(79, edition.CurrentStock);
        Assert.NotNull(dto);
    }

    [Fact]
    public async Task ExtendLoanAsync_WhenLoanMissing_ThrowsLibraryException()
    {
        SetupDefaultSettings();
        _loanRepo.Setup(l => l.GetByIdAsync(1)).ReturnsAsync((Loan)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.ExtendLoanAsync(new ExtensionRequest { LoanId = 1, DaysRequested = 3 }));
    }

    [Fact]
    public async Task ExtendLoanAsync_WhenTotalExtensionsExceedLim_Throws()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var loan = new Loan { Id = 10, ReaderId = reader.Id, DueDate = DateTime.Now.AddDays(5) };

        _loanRepo.Setup(l => l.GetByIdAsync(loan.Id)).ReturnsAsync(loan);
        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);

        _loanRepo.Setup(l => l.GetTotalExtensionDaysInLastThreeMonthsAsync(reader.Id))
            .ReturnsAsync(19); // LIM=20, +2 => 21

        var sut = CreateSut();

        await Assert.ThrowsAsync<LoanRuleViolationException>(() => sut.ExtendLoanAsync(new ExtensionRequest { LoanId = loan.Id, DaysRequested = 2 }));
    }

    [Fact]
    public async Task ExtendLoanAsync_WhenAllowed_UpdatesDueDate_AndCallsUpdate()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var loan = new Loan { Id = 10, ReaderId = reader.Id, DueDate = DateTime.Now.AddDays(5) };

        _loanRepo.Setup(l => l.GetByIdAsync(loan.Id)).ReturnsAsync(loan);
        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);

        _loanRepo.Setup(l => l.GetTotalExtensionDaysInLastThreeMonthsAsync(reader.Id))
            .ReturnsAsync(10);

        var oldDue = loan.DueDate;

        var sut = CreateSut();

        await sut.ExtendLoanAsync(new ExtensionRequest { LoanId = loan.Id, DaysRequested = 3 });

        Assert.Equal(oldDue.AddDays(3), loan.DueDate);
        _loanRepo.Verify(l => l.UpdateAsync(It.Is<Loan>(x => x.Id == loan.Id)), Times.Once);
    }

    [Fact]
    public async Task ReturnBookAsync_WhenLoanMissing_ThrowsLibraryException()
    {
        SetupDefaultSettings();
        _loanRepo.Setup(l => l.GetByIdAsync(1)).ReturnsAsync((Loan)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.ReturnBookAsync(1));
    }

    [Fact]
    public async Task ReturnBookAsync_WhenAlreadyReturned_ThrowsLibraryException()
    {
        SetupDefaultSettings();
        var loan = new Loan { Id = 1, ReturnDate = DateTime.Now.AddDays(-1) };
        _loanRepo.Setup(l => l.GetByIdAsync(1)).ReturnsAsync(loan);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.ReturnBookAsync(1));
    }

    [Fact]
    public async Task ReturnBookAsync_WhenValid_SetsReturnDate_IncrementsEditionStock_CallsUpdate()
    {
        SetupDefaultSettings();

        var edition = Edition(current: 10);
        var loan = new Loan { Id = 1, Edition = edition, ReturnDate = null };

        _loanRepo.Setup(l => l.GetByIdAsync(1)).ReturnsAsync(loan);

        var sut = CreateSut();

        await sut.ReturnBookAsync(1);

        Assert.NotNull(loan.ReturnDate);
        Assert.Equal(11, edition.CurrentStock);
        _loanRepo.Verify(l => l.UpdateAsync(It.Is<Loan>(x => x.Id == 1)), Times.Once);
    }

    private static List<Loan> Loans(int count) => Enumerable.Range(0, count).Select(_ => new Loan()).ToList();

    private void SetupBorrowHappyPath(
        Reader reader,
        Edition edition,
        int loansToday = 0,
        int loansInPer = 0,
        Loan? lastLoanForBook = null)
    {
        SetupDefaultSettings();

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        // "today" check
        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(Loans(loansToday));

        // "PER window" check (anything not exactly DateTime.Today start)
        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(
                reader.Id,
                It.Is<DateTime>(d => d.Date < DateTime.Today),
                It.IsAny<DateTime>()))
            .ReturnsAsync(Loans(loansInPer));

        _loanRepo.Setup(l => l.GetLastLoanForBookAsync(reader.Id, edition.BookId))
            .ReturnsAsync(lastLoanForBook);

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.LoanDetailsDto>(It.IsAny<Loan>()))
            .Returns(new ServiceLayer.DTOs.Responses.LoanDetailsDto { Id = 1, DueDate = DateTime.Now.AddDays(14) });
    }

    // 1
    [Fact]
    public async Task BorrowBookAsync_WhenReaderMissing_DoesNotCallBookRepo()
    {
        SetupDefaultSettings();
        _readerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Reader)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = 1, EditionId = 1 }));

        _bookRepo.Verify(b => b.GetEditionByIdAsync(It.IsAny<int>()), Times.Never);
        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Never);
    }

    // 2
    [Fact]
    public async Task BorrowBookAsync_WhenEditionMissing_DoesNotAddLoan()
    {
        SetupDefaultSettings();
        _readerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Reader());
        _bookRepo.Setup(r => r.GetEditionByIdAsync(1)).ReturnsAsync((Edition)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = 1, EditionId = 1 }));

        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Never);
    }

    // 3
    [Fact]
    public async Task BorrowBookAsync_WhenLoansTodayAtNCZ_DoesNotAddLoan_AndStockUnchanged()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var edition = Edition(current: 80);

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(Loans(2)); // NCZ=2

        var sut = CreateSut();

        await Assert.ThrowsAsync<LoanRuleViolationException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id }));

        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Never);
        Assert.Equal(80, edition.CurrentStock);
    }

    // 4
    [Fact]
    public async Task BorrowBookAsync_WhenStaffLoansTodayAtPERSIMP_DoesNotAddLoan()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: true);
        var edition = Edition(current: 80);

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(Loans(10)); // PERSIMP=10

        var sut = CreateSut();

        await Assert.ThrowsAsync<LoanRuleViolationException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id }));

        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Never);
    }

    // 5
    [Fact]
    public async Task BorrowBookAsync_WhenLoansInPERAtNMC_DoesNotAddLoan()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var edition = Edition(current: 80);

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan>());

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, It.Is<DateTime>(d => d.Date < DateTime.Today), It.IsAny<DateTime>()))
            .ReturnsAsync(Loans(5)); // NMC=5

        var sut = CreateSut();

        await Assert.ThrowsAsync<LoanRuleViolationException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id }));

        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Never);
    }

    // 6
    [Fact]
    public async Task BorrowBookAsync_WhenDeltaNotPassed_DoesNotAddLoan()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var edition = Edition(bookId: 99, current: 80);

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan>());
        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, It.Is<DateTime>(d => d.Date < DateTime.Today), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan>());

        _loanRepo.Setup(l => l.GetLastLoanForBookAsync(reader.Id, edition.BookId))
            .ReturnsAsync(new Loan { LoanDate = DateTime.Now.AddDays(-3) }); // DELTA=14

        var sut = CreateSut();

        await Assert.ThrowsAsync<LoanRuleViolationException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id }));

        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Never);
    }

    // 7
    [Fact]
    public async Task BorrowBookAsync_WhenStockBelow10Percent_DoesNotAddLoan_AndStockUnchanged()
    {
        SetupDefaultSettings();

        var reader = Reader();
        var edition = Edition(initial: 100, current: 20, readingOnly: 11); // available=9 < 10%

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Loan>());
        _loanRepo.Setup(l => l.GetLastLoanForBookAsync(reader.Id, edition.BookId))
            .ReturnsAsync((Loan)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<InsufficientStockException>(() => sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id }));

        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Never);
        Assert.Equal(20, edition.CurrentStock);
    }

    // 8
    [Fact]
    public async Task BorrowBookAsync_WhenAllRulesPass_CallsMapperOnce()
    {
        var reader = Reader(staff: false);
        var edition = Edition(initial: 100, current: 80, readingOnly: 0);

        SetupBorrowHappyPath(reader, edition, loansToday: 0, loansInPer: 0, lastLoanForBook: null);

        var sut = CreateSut();

        await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        _mapper.Verify(m => m.Map<ServiceLayer.DTOs.Responses.LoanDetailsDto>(It.IsAny<Loan>()), Times.Once);
    }

    // 9
    [Fact]
    public async Task BorrowBookAsync_WhenAllRulesPass_AddsLoanWithCorrectIds()
    {
        var reader = Reader(id: 7, staff: false);
        var edition = Edition(id: 3, bookId: 99, initial: 100, current: 80, readingOnly: 0);

        SetupBorrowHappyPath(reader, edition, loansToday: 0, loansInPer: 0, lastLoanForBook: null);

        Loan? captured = null;
        _loanRepo.Setup(l => l.AddAsync(It.IsAny<Loan>()))
            .Callback<Loan>(x => captured = x)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        Assert.NotNull(captured);
        Assert.Equal(reader.Id, captured!.ReaderId);
        Assert.Equal(edition.Id, captured.EditionId);
    }

    // 10
    [Fact]
    public async Task BorrowBookAsync_CallsGetLoansTodayWithDateTimeToday()
    {
        var reader = Reader(id: 2, staff: false);
        var edition = Edition(id: 5, bookId: 10, initial: 100, current: 80, readingOnly: 0);

        SetupBorrowHappyPath(reader, edition, loansToday: 0, loansInPer: 0, lastLoanForBook: null);

        var sut = CreateSut();

        await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        _loanRepo.Verify(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()), Times.Once);
    }

    // 11
    [Fact]
    public async Task BorrowBookAsync_CallsGetLastLoanForBook_WithCorrectBookId()
    {
        var reader = Reader(id: 2, staff: false);
        var edition = Edition(id: 5, bookId: 123, initial: 100, current: 80, readingOnly: 0);

        SetupBorrowHappyPath(reader, edition, loansToday: 0, loansInPer: 0, lastLoanForBook: null);

        var sut = CreateSut();

        await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        _loanRepo.Verify(l => l.GetLastLoanForBookAsync(reader.Id, 123), Times.Once);
    }

    // 12
    [Fact]
    public async Task BorrowBookAsync_WhenLastLoanForBookIsNull_AllowsBorrow()
    {
        var reader = Reader(staff: false);
        var edition = Edition(initial: 100, current: 80, readingOnly: 0);

        SetupBorrowHappyPath(reader, edition, loansToday: 0, loansInPer: 0, lastLoanForBook: null);

        var sut = CreateSut();

        var dto = await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        Assert.NotNull(dto);
        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Once);
    }

    // 13
    [Fact]
    public async Task BorrowBookAsync_WhenDeltaExactlyPassed_DoesNotThrow()
    {
        var reader = Reader(staff: false);
        var edition = Edition(bookId: 44, initial: 100, current: 80, readingOnly: 0);

        var lastLoan = new Loan { LoanDate = DateTime.Now.AddDays(-14) }; // DELTA=14
        SetupBorrowHappyPath(reader, edition, loansToday: 0, loansInPer: 0, lastLoanForBook: lastLoan);

        var sut = CreateSut();

        var dto = await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        Assert.NotNull(dto);
        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Once);
    }

    // 14
    [Fact]
    public async Task BorrowBookAsync_WhenLoansTodayExactlyNCZMinus1_AllowsBorrow()
    {
        var reader = Reader(staff: false);
        var edition = Edition(initial: 100, current: 80, readingOnly: 0);

        SetupBorrowHappyPath(reader, edition, loansToday: 1 /* NCZ=2 */, loansInPer: 0, lastLoanForBook: null);

        var sut = CreateSut();

        var dto = await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        Assert.NotNull(dto);
        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Once);
    }

    // 15
    [Fact]
    public async Task BorrowBookAsync_WhenLoansInPERExactlyNMCMinus1_AllowsBorrow()
    {
        var reader = Reader(staff: false);
        var edition = Edition(initial: 100, current: 80, readingOnly: 0);

        SetupBorrowHappyPath(reader, edition, loansToday: 0, loansInPer: 4 /* NMC=5 */, lastLoanForBook: null);

        var sut = CreateSut();

        var dto = await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        Assert.NotNull(dto);
        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Once);
    }

    // 16
    [Fact]
    public async Task BorrowBookAsync_WhenStaffLoansTodayExactlyPERSIMPMinus1_AllowsBorrow()
    {
        var reader = Reader(staff: true);
        var edition = Edition(initial: 100, current: 80, readingOnly: 0);

        SetupBorrowHappyPath(reader, edition, loansToday: 9 /* PERSIMP=10 */, loansInPer: 0, lastLoanForBook: null);

        var sut = CreateSut();

        var dto = await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        Assert.NotNull(dto);
        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Once);
    }

    // 17
    [Fact]
    public async Task BorrowBookAsync_WhenStockExactly10Percent_AllowsBorrow()
    {
        var reader = Reader(staff: false);
        // initial=100 => 10% = 10; availableNonReadingRoom = current - readingOnly = 10
        var edition = Edition(initial: 100, current: 10, readingOnly: 0);

        SetupBorrowHappyPath(reader, edition, loansToday: 0, loansInPer: 0, lastLoanForBook: null);

        var sut = CreateSut();

        var dto = await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        Assert.NotNull(dto);
        Assert.Equal(9, edition.CurrentStock);
    }

    // 18
    [Fact]
    public async Task ExtendLoanAsync_WhenReaderMissing_ThrowsLibraryException()
    {
        SetupDefaultSettings();

        var loan = new Loan { Id = 10, ReaderId = 55, DueDate = DateTime.Now.AddDays(5) };

        _loanRepo.Setup(l => l.GetByIdAsync(loan.Id)).ReturnsAsync(loan);
        _readerRepo.Setup(r => r.GetByIdAsync(loan.ReaderId)).ReturnsAsync((Reader)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.ExtendLoanAsync(new ExtensionRequest { LoanId = loan.Id, DaysRequested = 3 }));

        _loanRepo.Verify(l => l.UpdateAsync(It.IsAny<Loan>()), Times.Never);
    }

    // 19
    [Fact]
    public async Task ExtendLoanAsync_WhenAllowed_CallsGetTotalExtensionsWithReaderId()
    {
        SetupDefaultSettings();

        var reader = Reader(id: 9, staff: false);
        var loan = new Loan { Id = 10, ReaderId = reader.Id, DueDate = DateTime.Now.AddDays(5) };

        _loanRepo.Setup(l => l.GetByIdAsync(loan.Id)).ReturnsAsync(loan);
        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _loanRepo.Setup(l => l.GetTotalExtensionDaysInLastThreeMonthsAsync(reader.Id)).ReturnsAsync(0);

        var sut = CreateSut();

        await sut.ExtendLoanAsync(new ExtensionRequest { LoanId = loan.Id, DaysRequested = 3 });

        _loanRepo.Verify(l => l.GetTotalExtensionDaysInLastThreeMonthsAsync(reader.Id), Times.Once);
    }

    // 20
    [Fact]
    public async Task ExtendLoanAsync_WhenTotalPlusRequestedEqualsLim_AllowsAndUpdates()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var loan = new Loan { Id = 10, ReaderId = reader.Id, DueDate = DateTime.Now.AddDays(5) };

        _loanRepo.Setup(l => l.GetByIdAsync(loan.Id)).ReturnsAsync(loan);
        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);

        _loanRepo.Setup(l => l.GetTotalExtensionDaysInLastThreeMonthsAsync(reader.Id))
            .ReturnsAsync(18); // LIM=20; request 2 => exactly 20

        var sut = CreateSut();

        var oldDue = loan.DueDate;
        await sut.ExtendLoanAsync(new ExtensionRequest { LoanId = loan.Id, DaysRequested = 2 });

        Assert.Equal(oldDue.AddDays(2), loan.DueDate);
        _loanRepo.Verify(l => l.UpdateAsync(It.Is<Loan>(x => x.Id == loan.Id)), Times.Once);
    }

    // 21
    [Fact]
    public async Task ExtendLoanAsync_WhenExceedLim_DoesNotCallUpdate()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var loan = new Loan { Id = 10, ReaderId = reader.Id, DueDate = DateTime.Now.AddDays(5) };

        _loanRepo.Setup(l => l.GetByIdAsync(loan.Id)).ReturnsAsync(loan);
        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);

        _loanRepo.Setup(l => l.GetTotalExtensionDaysInLastThreeMonthsAsync(reader.Id))
            .ReturnsAsync(20); // LIM=20; request 1 => 21

        var sut = CreateSut();

        await Assert.ThrowsAsync<LoanRuleViolationException>(() =>
            sut.ExtendLoanAsync(new ExtensionRequest { LoanId = loan.Id, DaysRequested = 1 }));

        _loanRepo.Verify(l => l.UpdateAsync(It.IsAny<Loan>()), Times.Never);
    }

    // 22
    [Fact]
    public async Task ExtendLoanAsync_WhenAllowed_DoesNotSetReturnDate()
    {
        SetupDefaultSettings();

        var reader = Reader(staff: false);
        var loan = new Loan { Id = 10, ReaderId = reader.Id, DueDate = DateTime.Now.AddDays(5), ReturnDate = null };

        _loanRepo.Setup(l => l.GetByIdAsync(loan.Id)).ReturnsAsync(loan);
        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _loanRepo.Setup(l => l.GetTotalExtensionDaysInLastThreeMonthsAsync(reader.Id)).ReturnsAsync(0);

        var sut = CreateSut();

        await sut.ExtendLoanAsync(new ExtensionRequest { LoanId = loan.Id, DaysRequested = 3 });

        Assert.Null(loan.ReturnDate);
    }

    // 23
    [Fact]
    public async Task ReturnBookAsync_WhenLoanMissing_DoesNotCallUpdate()
    {
        SetupDefaultSettings();
        _loanRepo.Setup(l => l.GetByIdAsync(1)).ReturnsAsync((Loan)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.ReturnBookAsync(1));

        _loanRepo.Verify(l => l.UpdateAsync(It.IsAny<Loan>()), Times.Never);
    }

    // 24
    [Fact]
    public async Task ReturnBookAsync_WhenAlreadyReturned_DoesNotCallUpdate_AndDoesNotChangeStock()
    {
        SetupDefaultSettings();

        var edition = Edition(current: 10);
        var loan = new Loan { Id = 1, Edition = edition, ReturnDate = DateTime.Now.AddDays(-1) };

        _loanRepo.Setup(l => l.GetByIdAsync(1)).ReturnsAsync(loan);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.ReturnBookAsync(1));

        _loanRepo.Verify(l => l.UpdateAsync(It.IsAny<Loan>()), Times.Never);
        Assert.Equal(10, edition.CurrentStock);
    }

    // 25
    [Fact]
    public async Task ReturnBookAsync_WhenValid_CallsUpdateOnce()
    {
        SetupDefaultSettings();

        var edition = Edition(current: 10);
        var loan = new Loan { Id = 1, Edition = edition, ReturnDate = null };

        _loanRepo.Setup(l => l.GetByIdAsync(1)).ReturnsAsync(loan);

        var sut = CreateSut();

        await sut.ReturnBookAsync(1);

        _loanRepo.Verify(l => l.UpdateAsync(It.Is<Loan>(x => x.Id == 1)), Times.Once);
    }

    // 26
    [Fact]
    public async Task ReturnBookAsync_WhenValid_IncrementsEditionStockByOne()
    {
        SetupDefaultSettings();

        var edition = Edition(current: 10);
        var loan = new Loan { Id = 1, Edition = edition, ReturnDate = null };

        _loanRepo.Setup(l => l.GetByIdAsync(1)).ReturnsAsync(loan);

        var sut = CreateSut();

        await sut.ReturnBookAsync(1);

        Assert.Equal(11, edition.CurrentStock);
    }

    // 27
    [Fact]
    public async Task ReturnBookAsync_WhenValid_SetsReturnDateWithinExecutionWindow()
    {
        SetupDefaultSettings();

        var edition = Edition(current: 10);
        var loan = new Loan { Id = 1, Edition = edition, ReturnDate = null };

        _loanRepo.Setup(l => l.GetByIdAsync(1)).ReturnsAsync(loan);

        var sut = CreateSut();

        var before = DateTime.Now;
        await sut.ReturnBookAsync(1);
        var after = DateTime.Now;

        Assert.NotNull(loan.ReturnDate);
        Assert.True(loan.ReturnDate >= before && loan.ReturnDate <= after);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenStaff_UsesHalfPER_AndDoubleNMC_AllowsBorrow()
    {
        // Settings default: NMC=5, PER=30
        // Staff => NMC=10, PER=15

        var reader = Reader(id: 100, staff: true);
        var edition = Edition(id: 200, bookId: 300, initial: 100, current: 80, readingOnly: 0);

        SetupDefaultSettings();

        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);
        _bookRepo.Setup(b => b.GetEditionByIdAsync(edition.Id)).ReturnsAsync(edition);

        // today ok
        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(reader.Id, DateTime.Today, It.IsAny<DateTime>()))
            .ReturnsAsync(Loans(0));

        // capture startDate used for PER calculation
        DateTime? capturedPerStart = null;
        _loanRepo.Setup(l => l.GetLoansInPeriodAsync(
                reader.Id,
                It.Is<DateTime>(d => d.Date < DateTime.Today),
                It.IsAny<DateTime>()))
            .Callback<int, DateTime, DateTime>((_, start, __) => capturedPerStart = start)
            .ReturnsAsync(Loans(6)); // non-staff would fail (6 > 5), staff should pass (6 <= 10)

        _loanRepo.Setup(l => l.GetLastLoanForBookAsync(reader.Id, edition.BookId))
            .ReturnsAsync((Loan)null!);

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.LoanDetailsDto>(It.IsAny<Loan>()))
            .Returns(new ServiceLayer.DTOs.Responses.LoanDetailsDto { Id = 1, DueDate = DateTime.Now.AddDays(14) });

        var sut = CreateSut();

        var dto = await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        Assert.NotNull(dto);
        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Once);

        // PER should be halved: start date ≈ today - 15 days (toleranță 1 zi pt. DateTime.Now/Today)
        Assert.NotNull(capturedPerStart);
        var expected = DateTime.Today.AddDays(-15);
        Assert.True(
            capturedPerStart!.Value.Date == expected.Date
            || capturedPerStart!.Value.Date == expected.AddDays(-1).Date
            || capturedPerStart!.Value.Date == expected.AddDays(1).Date);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenStaff_DeltaIsHalved_AllowsBorrowSooner()
    {
        // Settings default: DELTA=14
        // Staff => DELTA=7
        // Last loan was 8 days ago -> staff should be allowed (8 >= 7),
        // while non-staff would be blocked (8 < 14).

        var reader = Reader(id: 101, staff: true);
        var edition = Edition(id: 201, bookId: 301, initial: 100, current: 80, readingOnly: 0);

        var lastLoan = new Loan { LoanDate = DateTime.Now.AddDays(-8) };

        SetupBorrowHappyPath(reader, edition, loansToday: 0, loansInPer: 0, lastLoanForBook: lastLoan);

        var sut = CreateSut();

        var dto = await sut.BorrowBookAsync(new BorrowRequest { ReaderId = reader.Id, EditionId = edition.Id });

        Assert.NotNull(dto);
        _loanRepo.Verify(l => l.AddAsync(It.IsAny<Loan>()), Times.Once);
    }

    [Fact]
    public async Task ExtendLoanAsync_WhenStaff_LimIsDoubled_AllowsExtension()
    {
        // Settings default: LIM=20
        // Staff => LIM=40
        // Total extensions last 3 months = 21; request 2 => 23
        // Non-staff would fail (23 > 20), staff should pass (23 <= 40).

        SetupDefaultSettings();

        var reader = Reader(id: 202, staff: true);
        var loan = new Loan { Id = 303, ReaderId = reader.Id, DueDate = DateTime.Now.AddDays(5) };

        _loanRepo.Setup(l => l.GetByIdAsync(loan.Id)).ReturnsAsync(loan);
        _readerRepo.Setup(r => r.GetByIdAsync(reader.Id)).ReturnsAsync(reader);

        _loanRepo.Setup(l => l.GetTotalExtensionDaysInLastThreeMonthsAsync(reader.Id))
            .ReturnsAsync(21);

        var oldDue = loan.DueDate;

        var sut = CreateSut();

        await sut.ExtendLoanAsync(new ExtensionRequest { LoanId = loan.Id, DaysRequested = 2 });

        Assert.Equal(oldDue.AddDays(2), loan.DueDate);
        _loanRepo.Verify(l => l.UpdateAsync(It.Is<Loan>(x => x.Id == loan.Id)), Times.Once);
    }
}