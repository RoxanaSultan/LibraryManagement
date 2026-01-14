using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DomainModel.Entities;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.DTOs.Responses;
using ServiceLayer.Mappings;
using Xunit;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        // IMPORTANT: validează toate mapările -> crește coverage și prinde erori
        config.AssertConfigurationIsValid();

        _mapper = config.CreateMapper();
    }

    [Fact]
    public void ReaderCreateRequest_To_Reader_MapsIsStaffToIsLibraryStaff()
    {
        // Arrange
        var request = new ReaderCreateRequest
        {
            // dacă ai și alte câmpuri, le poți seta aici
            IsStaff = true
        };

        // Act
        var entity = _mapper.Map<Reader>(request);

        // Assert
        Assert.True(entity.IsLibraryStaff);

        // și un caz invers, ca să acoperi mai bine
        request.IsStaff = false;
        entity = _mapper.Map<Reader>(request);
        Assert.False(entity.IsLibraryStaff);
    }

    [Fact]
    public void Reader_To_ReaderDetailsDto_MapsFullNameAndIsStaffAccount()
    {
        // Arrange
        var reader = new Reader
        {
            FirstName = "Ana",
            LastName = "Popescu",
            IsLibraryStaff = true,
            Email = "ana@ex.com",
            PhoneNumber = "0712345678"
        };

        // Act
        var dto = _mapper.Map<ReaderDetailsDto>(reader);

        // Assert
        Assert.Equal("Ana Popescu", dto.FullName);
        Assert.True(dto.IsStaffAccount);
    }

    [Fact]
    public void Reader_To_ReaderDetailsDto_ContactInfo_PrefersEmail_WhenNotEmpty()
    {
        // Arrange
        var reader = new Reader
        {
            FirstName = "Ana",
            LastName = "Popescu",
            Email = "ana@ex.com",
            PhoneNumber = "0712345678"
        };

        // Act
        var dto = _mapper.Map<ReaderDetailsDto>(reader);

        // Assert
        Assert.Equal("ana@ex.com", dto.ContactInfo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Reader_To_ReaderDetailsDto_ContactInfo_FallsBackToPhone_WhenEmailNullOrEmpty(string email)
    {
        // Arrange
        var reader = new Reader
        {
            FirstName = "Ana",
            LastName = "Popescu",
            Email = email,
            PhoneNumber = "0712345678"
        };

        // Act
        var dto = _mapper.Map<ReaderDetailsDto>(reader);

        // Assert
        Assert.Equal("0712345678", dto.ContactInfo);
    }

    [Fact]
    public void Book_To_BookListDto_MapsIdsAuthorsDomainsAndTotalAvailableCopies()
    {
        // Arrange
        var book = new Book
        {
            Id = 10,
            Title = "Clean Code",
            Authors = new List<Author>
            {
                new Author { Name = "Robert C. Martin" },
                new Author { Name = "Alt Autor" }
            },
            ExplicitDomains = new List<Domain>
            {
                new Domain { Name = "Software" },
                new Domain { Name = "Engineering" }
            },
            Editions = new List<Edition>
            {
                new Edition { CurrentStock = 3 },
                new Edition { CurrentStock = 7 }
            }
        };

        // Act
        var dto = _mapper.Map<BookListDto>(book);

        // Assert
        Assert.Equal(10, dto.BookId);
        Assert.Equal(new[] { "Robert C. Martin", "Alt Autor" }, dto.Authors);
        Assert.Equal(new[] { "Software", "Engineering" }, dto.Domains);
        Assert.Equal(10, dto.TotalAvailableCopies);
    }

    [Fact]
    public void Loan_To_LoanDetailsDto_Status_Returnat_WhenReturnDateHasValue()
    {
        // Arrange
        var loan = new Loan
        {
            DueDate = DateTime.Now.AddDays(-10),
            ReturnDate = DateTime.Now.AddDays(-1),
            Edition = new Edition
            {
                Book = new Book { Title = "Dune" }
            }
        };

        // Act
        var dto = _mapper.Map<LoanDetailsDto>(loan);

        // Assert
        Assert.Equal("Dune", dto.BookTitle);
        Assert.Equal("Returnat", dto.Status);
    }

    [Fact]
    public void Loan_To_LoanDetailsDto_Status_Intarziat_WhenOverdueAndNotReturned()
    {
        // Arrange
        var loan = new Loan
        {
            DueDate = DateTime.Now.AddDays(-1), // deja trecut
            ReturnDate = null,
            Edition = new Edition
            {
                Book = new Book { Title = "Dune" }
            }
        };

        // Act
        var dto = _mapper.Map<LoanDetailsDto>(loan);

        // Assert
        Assert.Equal("Intarziat", dto.Status);
    }

    [Fact]
    public void Loan_To_LoanDetailsDto_Status_Activ_WhenNotOverdueAndNotReturned()
    {
        // Arrange
        var loan = new Loan
        {
            DueDate = DateTime.Now.AddDays(3), // încă în termen
            ReturnDate = null,
            Edition = new Edition
            {
                Book = new Book { Title = "Dune" }
            }
        };

        // Act
        var dto = _mapper.Map<LoanDetailsDto>(loan);

        // Assert
        Assert.Equal("Activ", dto.Status);
    }
}