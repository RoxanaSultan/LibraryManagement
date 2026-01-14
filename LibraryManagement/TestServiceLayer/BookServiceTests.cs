using AutoMapper;
using DomainModel.Entities;
using DomainModel.Exceptions;
using DomainModel.Interfaces;
using Moq;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.Interfaces;
using ServiceLayer.Services;

namespace TestServiceLayer;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepository = new();
    private readonly Mock<IDomainRepository> _domainRepository = new();
    private readonly Mock<ILibrarySettings> _settings = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ILoggerService> _logger = new();

    private BookService CreateSut()
        => new BookService(_bookRepository.Object, _domainRepository.Object, _settings.Object, _mapper.Object, _logger.Object);

    [Fact]
    public async Task AddBookAsync_WhenDomainCountExceedsDOMENII_ThrowsDomainConstraintException()
    {
        _settings.SetupGet(s => s.DOMENII).Returns(2);

        var request = new BookCreateRequest
        {
            Title = "T",
            DomainIds = new List<int> { 1, 2, 3 }
        };

        var sut = CreateSut();

        await Assert.ThrowsAsync<DomainConstraintException>(() => sut.AddBookAsync(request));
        _bookRepository.Verify(r => r.AddAsync(It.IsAny<Book>()), Times.Never);
    }

    [Fact]
    public async Task AddBookAsync_WhenDomainsAreSiblings_AllowsAdd()
    {
        _settings.SetupGet(s => s.DOMENII).Returns(3);

        var d1 = new Domain { Id = 1, Name = "Algoritmi", ParentDomainId = 10, ParentDomain = new Domain { Id = 10, Name = "Info" } };
        var d2 = new Domain { Id = 2, Name = "Baze de date", ParentDomainId = 10, ParentDomain = new Domain { Id = 10, Name = "Info" } };

        _domainRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(d1);
        _domainRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(d2);

        _mapper.Setup(m => m.Map<Book>(It.IsAny<BookCreateRequest>()))
            .Returns((BookCreateRequest req) => new Book { Title = req.Title });

        var sut = CreateSut();

        await sut.AddBookAsync(new BookCreateRequest { Title = "OK", DomainIds = new List<int> { 1, 2 } });

        _bookRepository.Verify(r => r.AddAsync(It.Is<Book>(b => b.Title == "OK")), Times.Once);
        _logger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Carte adaugata cu succes"))), Times.Once);
    }

    [Fact]
    public async Task AddBookAsync_WhenAncestorAndDescendantSpecified_ThrowsDomainHierarchyException()
    {
        _settings.SetupGet(s => s.DOMENII).Returns(5);

        var science = new Domain { Id = 1, Name = "Stiinta" };
        var info = new Domain { Id = 2, Name = "Informatica", ParentDomainId = 1, ParentDomain = science };
        var db = new Domain { Id = 3, Name = "Baze de date", ParentDomainId = 2, ParentDomain = info };

        _domainRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(science);
        _domainRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(db);

        _domainRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(info);

        var sut = CreateSut();

        var request = new BookCreateRequest
        {
            Title = "Bad",
            DomainIds = new List<int> { 1, 3 }
        };

        await Assert.ThrowsAsync<DomainHierarchyException>(() => sut.AddBookAsync(request));
        _bookRepository.Verify(r => r.AddAsync(It.IsAny<Book>()), Times.Never);
        _logger.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Incalcare ierarhie"))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task AddBookAsync_WhenDuplicateDomainIds_DoesNotThrow_AndAdds()
    {
        _settings.SetupGet(s => s.DOMENII).Returns(3);

        var d1 = new Domain { Id = 1, Name = "Info" };
        _domainRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(d1);

        _mapper.Setup(m => m.Map<Book>(It.IsAny<BookCreateRequest>()))
            .Returns((BookCreateRequest req) => new Book { Title = req.Title });

        var sut = CreateSut();

        await sut.AddBookAsync(new BookCreateRequest { Title = "Dup", DomainIds = new List<int> { 1, 1 } });

        _bookRepository.Verify(r => r.AddAsync(It.Is<Book>(b => b.Title == "Dup")), Times.Once);
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsMappedDtos()
    {
        _bookRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Book> { new Book { Id = 1, Title = "A" }, new Book { Id = 2, Title = "B" } });

        _mapper.Setup(m => m.Map<IEnumerable<ServiceLayer.DTOs.Responses.BookListDto>>(It.IsAny<IEnumerable<Book>>()))
            .Returns(new List<ServiceLayer.DTOs.Responses.BookListDto>
            {
                new() { BookId = 1, Title = "A" },
                new() { BookId = 2, Title = "B" }
            });

        var sut = CreateSut();

        var result = await sut.GetAllBooksAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, x => x.Title == "A");
    }
}