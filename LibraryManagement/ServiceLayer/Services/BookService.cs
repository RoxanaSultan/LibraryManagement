using AutoMapper;
using DomainModel.Entities;
using DomainModel.Exceptions;
using DomainModel.Interfaces;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.DTOs.Responses;
using ServiceLayer.Interfaces;

namespace ServiceLayer.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IDomainRepository _domainRepository;
    private readonly ILibrarySettings _settings;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public BookService(
        IBookRepository bookRepository,
        IDomainRepository domainRepository,
        ILibrarySettings settings,
        IMapper mapper,
        ILoggerService logger)
    {
        _bookRepository = bookRepository;
        _domainRepository = domainRepository;
        _settings = settings;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task AddBookAsync(BookCreateRequest request)
    {
        _logger.LogInformation($"Incercare adaugare carte: {request.Title}");

        // 1. Verificam numarul maxim de domenii (Regula DOMENII)
        if (request.DomainIds.Count > _settings.DOMENII)
        {
            throw new DomainConstraintException($"O carte nu poate avea mai mult de {_settings.DOMENII} domenii.");
        }

        // 2. Verificam relatia stramos-descendent (Pagina 1)
        var domains = new List<Domain>();
        foreach (var id in request.DomainIds)
        {
            domains.Add(await _domainRepository.GetByIdAsync(id));
        }

        foreach (var d1 in domains)
        {
            foreach (var d2 in domains)
            {
                if (d1.Id == d2.Id) continue;

                // Daca d1 este stramosul lui d2 (sau invers)
                if (await IsAncestor(d1, d2))
                {
                    _logger.LogWarning($"Incalcare ierarhie: {d1.Name} este stramosul lui {d2.Name}");
                    throw new DomainHierarchyException($"Eroare: Domeniile {d1.Name} si {d2.Name} sunt in relatie stramos-descendent.");
                }
            }
        }

        // 3. Salvare (mapare si adaugare in repo)
        var book = _mapper.Map<Book>(request);
        await _bookRepository.AddAsync(book);
        _logger.LogInformation($"Carte adaugata cu succes: {book.Title}");
    }

    private async Task<bool> IsAncestor(Domain potentialAncestor, Domain potentialDescendant)
    {
        var current = potentialDescendant.ParentDomain;
        while (current != null)
        {
            if (current.Id == potentialAncestor.Id) return true;
            current = await _domainRepository.GetByIdAsync(current.ParentDomainId ?? 0);
        }
        return false;
    }

    public async Task<IEnumerable<BookListDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<BookListDto>>(books);
    }

    public Task<IEnumerable<BookListDto>> GetBooksByDomainAsync(int domainId)
    {
        // Aici se implementeaza logica de cautare recursiva in subdomenii
        throw new System.NotImplementedException();
    }
}