using AutoMapper;
using DomainModel.Entities;
using DomainModel.Exceptions;
using DomainModel.Interfaces;
using Moq;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.Interfaces;
using ServiceLayer.Services;

namespace TestServiceLayer;

public class ReaderServiceTests
{
    private readonly Mock<IReaderRepository> _readerRepo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ILoggerService> _logger = new();

    private ReaderService CreateSut()
        => new ReaderService(_readerRepo.Object, _mapper.Object, _logger.Object);

    [Fact]
    public async Task GetReaderByIdAsync_WhenNotFound_ThrowsLibraryException()
    {
        _readerRepo.Setup(r => r.GetByIdAsync(42)).ReturnsAsync((Reader)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.GetReaderByIdAsync(42));
        _logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetReaderByIdAsync_WhenFound_ReturnsMappedDto()
    {
        var reader = new Reader { Id = 1, FirstName = "Ana", LastName = "Pop", AccountId = "ACC" };
        _readerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reader);

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        var sut = CreateSut();

        var dto = await sut.GetReaderByIdAsync(1);

        Assert.NotNull(dto);
        Assert.Equal(1, dto.Id);
    }

    [Fact]
    public async Task RegisterReaderAsync_WhenAccountHasStaff_SetsIsLibraryStaffTrueBeforeSaving()
    {
        var request = new ReaderCreateRequest
        {
            FirstName = "Ana",
            LastName = "Pop",
            Address = "Strada 1",
            Email = "ana@test.com",
            PhoneNumber = "0712345678",
            AccountId = "ACC-1",
            IsStaff = false
        };

        // mapper: Request -> Reader
        _mapper.Setup(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()))
            .Returns((ReaderCreateRequest req) => new Reader
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Address = req.Address,
                Email = req.Email,
                PhoneNumber = req.PhoneNumber,
                AccountId = req.AccountId,
                IsLibraryStaff = req.IsStaff
            });

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        _readerRepo.Setup(r => r.GetReadersByAccountIdAsync("ACC-1"))
            .ReturnsAsync(new List<Reader> { new Reader { Id = 99, IsLibraryStaff = true, AccountId = "ACC-1", FirstName = "X", LastName = "Y" } });

        var sut = CreateSut();

        await sut.RegisterReaderAsync(request);

        _readerRepo.Verify(r => r.AddAsync(It.Is<Reader>(x => x.IsLibraryStaff == true)), Times.Once);
        _logger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RegisterReaderAsync_WhenAccountHasNoStaff_DoesNotForceStaff()
    {
        var request = new ReaderCreateRequest
        {
            FirstName = "Ana",
            LastName = "Pop",
            Address = "Strada 1",
            Email = "ana@test.com",
            PhoneNumber = "0712345678",
            AccountId = "ACC-2",
            IsStaff = false
        };

        _mapper.Setup(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()))
            .Returns((ReaderCreateRequest req) => new Reader
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Address = req.Address,
                Email = req.Email,
                PhoneNumber = req.PhoneNumber,
                AccountId = req.AccountId,
                IsLibraryStaff = req.IsStaff
            });

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        _readerRepo.Setup(r => r.GetReadersByAccountIdAsync("ACC-2"))
            .ReturnsAsync(new List<Reader> { new Reader { Id = 11, IsLibraryStaff = false, AccountId = "ACC-2" } });

        var sut = CreateSut();

        await sut.RegisterReaderAsync(request);

        _readerRepo.Verify(r => r.AddAsync(It.Is<Reader>(x => x.IsLibraryStaff == false)), Times.Once);
    }

    private static Reader Reader(int id = 1, string acc = "ACC", bool staff = false)
    => new Reader
    {
        Id = id,
        FirstName = "Ana",
        LastName = "Pop",
        Address = "Strada 1",
        Email = "ana@test.com",
        PhoneNumber = "0712345678",
        AccountId = acc,
        IsLibraryStaff = staff
    };

    private static ReaderCreateRequest CreateReq(string acc = "ACC", bool isStaff = false)
        => new ReaderCreateRequest
        {
            FirstName = "Ana",
            LastName = "Pop",
            Address = "Strada 1",
            Email = "ana@test.com",
            PhoneNumber = "0712345678",
            AccountId = acc,
            IsStaff = isStaff
        };

    // 1
    [Fact]
    public async Task GetReaderByIdAsync_WhenNotFound_LogsWarningOnlyOnce_AndDoesNotMap()
    {
        _readerRepo.Setup(r => r.GetByIdAsync(42)).ReturnsAsync((Reader)null!);

        var sut = CreateSut();

        await Assert.ThrowsAsync<LibraryException>(() => sut.GetReaderByIdAsync(42));

        _logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        _mapper.Verify(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()), Times.Never);
    }

    // 2
    [Fact]
    public async Task GetReaderByIdAsync_WhenFound_CallsRepoOnce()
    {
        var reader = Reader(id: 1);
        _readerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reader);

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(reader))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        var sut = CreateSut();

        await sut.GetReaderByIdAsync(1);

        _readerRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    // 3
    [Fact]
    public async Task GetReaderByIdAsync_WhenFound_CallsMapperWithSameEntity()
    {
        var reader = Reader(id: 1);
        _readerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reader);

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        var sut = CreateSut();

        await sut.GetReaderByIdAsync(1);

        _mapper.Verify(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.Is<Reader>(x => ReferenceEquals(x, reader))), Times.Once);
    }

    // 4
    [Fact]
    public async Task GetReaderByIdAsync_WhenFound_DoesNotLogWarning()
    {
        var reader = Reader(id: 1);
        _readerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reader);

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        var sut = CreateSut();

        await sut.GetReaderByIdAsync(1);

        _logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
    }

    // 5
    [Fact]
    public async Task RegisterReaderAsync_WhenRequestIsStaffTrue_SavesStaffTrue_EvenIfNoStaffOnAccount()
    {
        var req = CreateReq(acc: "ACC-S1", isStaff: true);

        _mapper.Setup(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()))
            .Returns((ReaderCreateRequest r) => new Reader
            {
                FirstName = r.FirstName,
                LastName = r.LastName,
                Address = r.Address,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                AccountId = r.AccountId,
                IsLibraryStaff = r.IsStaff
            });

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        _readerRepo.Setup(r => r.GetReadersByAccountIdAsync("ACC-S1"))
            .ReturnsAsync(new List<Reader>()); // no staff in account

        var sut = CreateSut();

        await sut.RegisterReaderAsync(req);

        _readerRepo.Verify(r => r.AddAsync(It.Is<Reader>(x => x.IsLibraryStaff == true && x.AccountId == "ACC-S1")), Times.Once);
    }

    // 6
    [Fact]
    public async Task RegisterReaderAsync_WhenAccountHasStaff_OverridesRequestIsStaffFalse_ToTrue()
    {
        var req = CreateReq(acc: "ACC-OVR", isStaff: false);

        _mapper.Setup(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()))
            .Returns((ReaderCreateRequest r) => new Reader
            {
                FirstName = r.FirstName,
                LastName = r.LastName,
                Address = r.Address,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                AccountId = r.AccountId,
                IsLibraryStaff = r.IsStaff // starts false
            });

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        _readerRepo.Setup(r => r.GetReadersByAccountIdAsync("ACC-OVR"))
            .ReturnsAsync(new List<Reader> { Reader(id: 99, acc: "ACC-OVR", staff: true) });

        var sut = CreateSut();

        await sut.RegisterReaderAsync(req);

        _readerRepo.Verify(r => r.AddAsync(It.Is<Reader>(x => x.AccountId == "ACC-OVR" && x.IsLibraryStaff == true)), Times.Once);
    }

    // 7
    [Fact]
    public async Task RegisterReaderAsync_WhenAccountHasNoStaff_DoesNotChangeRequestIsStaffFalse()
    {
        var req = CreateReq(acc: "ACC-NO", isStaff: false);

        _mapper.Setup(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()))
            .Returns((ReaderCreateRequest r) => new Reader
            {
                FirstName = r.FirstName,
                LastName = r.LastName,
                Address = r.Address,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                AccountId = r.AccountId,
                IsLibraryStaff = r.IsStaff
            });

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        _readerRepo.Setup(r => r.GetReadersByAccountIdAsync("ACC-NO"))
            .ReturnsAsync(new List<Reader> { Reader(id: 11, acc: "ACC-NO", staff: false) });

        var sut = CreateSut();

        await sut.RegisterReaderAsync(req);

        _readerRepo.Verify(r => r.AddAsync(It.Is<Reader>(x => x.AccountId == "ACC-NO" && x.IsLibraryStaff == false)), Times.Once);
    }

    // 8
    [Fact]
    public async Task RegisterReaderAsync_CallsGetReadersByAccountId_WithRequestAccountId()
    {
        var req = CreateReq(acc: "ACC-CALL", isStaff: false);

        _mapper.Setup(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()))
            .Returns((ReaderCreateRequest r) => new Reader { AccountId = r.AccountId, IsLibraryStaff = r.IsStaff });

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        _readerRepo.Setup(r => r.GetReadersByAccountIdAsync("ACC-CALL"))
            .ReturnsAsync(new List<Reader>());

        var sut = CreateSut();

        await sut.RegisterReaderAsync(req);

        _readerRepo.Verify(r => r.GetReadersByAccountIdAsync("ACC-CALL"), Times.Once);
    }

    // 9
    [Fact]
    public async Task RegisterReaderAsync_CallsAddAsyncOnce()
    {
        var req = CreateReq(acc: "ACC-ADD", isStaff: false);

        _mapper.Setup(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()))
            .Returns((ReaderCreateRequest r) => new Reader { AccountId = r.AccountId, IsLibraryStaff = r.IsStaff });

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        _readerRepo.Setup(r => r.GetReadersByAccountIdAsync("ACC-ADD"))
            .ReturnsAsync(new List<Reader>());

        var sut = CreateSut();

        await sut.RegisterReaderAsync(req);

        _readerRepo.Verify(r => r.AddAsync(It.IsAny<Reader>()), Times.Once);
    }

    // 10
    [Fact]
    public async Task RegisterReaderAsync_MapsRequestToReaderOnce_AndMapsReaderToDtoOnce()
    {
        var req = CreateReq(acc: "ACC-MAP", isStaff: false);

        _mapper.Setup(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()))
            .Returns((ReaderCreateRequest r) => new Reader { AccountId = r.AccountId, IsLibraryStaff = r.IsStaff });

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        _readerRepo.Setup(r => r.GetReadersByAccountIdAsync("ACC-MAP"))
            .ReturnsAsync(new List<Reader>());

        var sut = CreateSut();

        await sut.RegisterReaderAsync(req);

        _mapper.Verify(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()), Times.Once);
        _mapper.Verify(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()), Times.Once);
    }

    // 11
    [Fact]
    public async Task RegisterReaderAsync_WhenAccountHasStaff_LogsInformation()
    {
        var req = CreateReq(acc: "ACC-LOG", isStaff: false);

        _mapper.Setup(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()))
            .Returns((ReaderCreateRequest r) => new Reader { AccountId = r.AccountId, IsLibraryStaff = r.IsStaff });

        _mapper.Setup(m => m.Map<ServiceLayer.DTOs.Responses.ReaderDetailsDto>(It.IsAny<Reader>()))
            .Returns(new ServiceLayer.DTOs.Responses.ReaderDetailsDto { Id = 1, FullName = "Ana Pop" });

        _readerRepo.Setup(r => r.GetReadersByAccountIdAsync("ACC-LOG"))
            .ReturnsAsync(new List<Reader> { Reader(id: 5, acc: "ACC-LOG", staff: true) });

        var sut = CreateSut();

        await sut.RegisterReaderAsync(req);

        _logger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
    }

    // 12
    [Fact]
    public async Task RegisterReaderAsync_WhenMapperThrows_DoesNotCallRepo()
    {
        var req = CreateReq(acc: "ACC-ERR", isStaff: false);

        _mapper.Setup(m => m.Map<Reader>(It.IsAny<ReaderCreateRequest>()))
            .Throws(new InvalidOperationException("Mapping failed"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.RegisterReaderAsync(req));

        _readerRepo.Verify(r => r.GetReadersByAccountIdAsync(It.IsAny<string>()), Times.Never);
        _readerRepo.Verify(r => r.AddAsync(It.IsAny<Reader>()), Times.Never);
    }
}