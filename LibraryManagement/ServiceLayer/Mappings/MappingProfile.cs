using AutoMapper;
using DomainModel.Entities;
using ServiceLayer.DTOs.Requests;
using ServiceLayer.DTOs.Responses;

namespace ServiceLayer.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // --- Cititori ---

        CreateMap<ReaderCreateRequest, Reader>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsLibraryStaff, opt => opt.MapFrom(src => src.IsStaff));

        CreateMap<Reader, ReaderDetailsDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.ContactInfo, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.Email) ? src.Email : src.PhoneNumber))
            .ForMember(dest => dest.IsStaffAccount, opt => opt.MapFrom(src => src.IsLibraryStaff))
            .ForMember(dest => dest.ActiveLoansCount, opt => opt.Ignore());

        // --- Carti ---

        CreateMap<Book, BookListDto>()
            .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Authors, opt => opt.MapFrom(src =>
                src.Authors.Select(a => a.Name).ToList()))
            .ForMember(dest => dest.Domains, opt => opt.MapFrom(src =>
                src.ExplicitDomains.Select(d => d.Name).ToList()))
            .ForMember(dest => dest.TotalAvailableCopies, opt => opt.MapFrom(src =>
                src.Editions.Sum(e => e.CurrentStock)))
            .ForMember(dest => dest.IsAvailableForBorrowing, opt => opt.Ignore());

        // --- Imprumuturi ---

        CreateMap<Loan, LoanDetailsDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Edition.Book.Title))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.ReturnDate.HasValue ? "Returnat" :
                (System.DateTime.Now > src.DueDate ? "Intarziat" : "Activ")))
            .ForMember(dest => dest.ReaderName, opt => opt.Ignore());
    }
}