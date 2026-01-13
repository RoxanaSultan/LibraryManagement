using AutoMapper;
using DomainModel.Entities;
using DomainModel.Exceptions;
using DomainModel.Interfaces;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.DTOs.Responses;
using ServiceLayer.Interfaces;
using ServiceLayer.Validators;

namespace ServiceLayer.Services;

/// <summary>
/// Serviciu pentru gestionarea cititorilor bibliotecii.
/// </summary>
public class ReaderService : IReaderService
{
    private readonly IReaderRepository _readerRepository;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public ReaderService(
        IReaderRepository readerRepository,
        IMapper mapper,
        ILoggerService logger)
    {
        _readerRepository = readerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Inregistreaza un cititor nou in sistem.
    /// </summary>
    public async Task<ReaderDetailsDto> RegisterReaderAsync(ReaderCreateRequest request)
    {
        _logger.LogInformation($"Incepere proces inregistrare cititor: {request.FirstName} {request.LastName}");

        // 1. Validare date intrare (Validatorul din ServiceLayer creat anterior)
        var validator = new ReaderCreateRequestValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning($"Validare esuata pentru inregistrare cititor: {string.Join(", ", errors)}");
            throw new LibraryValidationException(errors);
        }

        // 2. Mapare la entitatea de domeniu
        var reader = _mapper.Map<Reader>(request);

        // 3. Regula "Cont partajat / Personal" (Pagina 2)
        // Daca exista deja cineva cu acest AccountId si e marcat ca staff, 
        // noul cititor va mosteni acest flag (sau logica specifica sistemului tau)
        var existingWithSameAccount = await _readerRepository.GetReadersByAccountIdAsync(request.AccountId);
        if (existingWithSameAccount.Any(r => r.IsLibraryStaff))
        {
            reader.IsLibraryStaff = true;
            _logger.LogInformation($"Cititorul a fost asociat unui cont de personal (AccountId: {request.AccountId})");
        }

        // 4. Persistenta
        await _readerRepository.AddAsync(reader);
        _logger.LogInformation($"Cititor creat cu succes. ID Generat: {reader.Id}");

        return _mapper.Map<ReaderDetailsDto>(reader);
    }

    /// <summary>
    /// Obtine detaliile unui cititor existent.
    /// </summary>
    public async Task<ReaderDetailsDto> GetReaderByIdAsync(int readerId)
    {
        var reader = await _readerRepository.GetByIdAsync(readerId);

        if (reader == null)
        {
            _logger.LogWarning($"Incercare acces cititor inexistent: ID {readerId}");
            throw new LibraryException($"Cititorul cu ID-ul {readerId} nu a fost gasit.");
        }

        return _mapper.Map<ReaderDetailsDto>(reader);
    }
}