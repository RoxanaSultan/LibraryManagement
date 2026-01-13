using AutoMapper;
using DomainModel.Entities;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.DTOs.Responses;

namespace ServiceLayer.Mappings;

/// <summary>
/// Configureaza regulile de mapare intre entitatile de domeniu si obiectele DTO.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Constructorul unde se definesc relatiile de transformare.
    /// </summary>
    public MappingProfile()
    {
        // --- Mapari pentru Cititori ---

        // Transforma cererea de creare intr-o entitate Reader
        CreateMap<ReaderCreateRequest, Reader>()
            .ForMember(dest => dest.IsLibraryStaff, opt => opt.MapFrom(src => src.IsStaff));

        // Transforma entitatea Reader in detaliile afisate in UI
        CreateMap<Reader, ReaderDetailsDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.ContactInfo, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.Email) ? src.Email : src.PhoneNumber))
            .ForMember(dest => dest.IsStaffAccount, opt => opt.MapFrom(src => src.IsLibraryStaff));


        // --- Mapari pentru Carti ---

        // Transforma entitatea Book in rezumatul pentru liste
        CreateMap<Book, BookListDto>()
            .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Authors, opt => opt.MapFrom(src =>
                src.Authors.Select(a => a.Name).ToList()))
            .ForMember(dest => dest.Domains, opt => opt.MapFrom(src =>
                src.ExplicitDomains.Select(d => d.Name).ToList()))
            .ForMember(dest => dest.TotalAvailableCopies, opt => opt.MapFrom(src =>
                src.Editions.Sum(e => e.CurrentStock)));


        // --- Mapari pentru Imprumuturi ---

        // Transforma entitatea Loan in detaliile tranzactiei
        CreateMap<Loan, LoanDetailsDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Edition.Book.Title))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.ReturnDate.HasValue ? "Returnat" :
                (System.DateTime.Now > src.DueDate ? "Intarziat" : "Activ")));
    }
}