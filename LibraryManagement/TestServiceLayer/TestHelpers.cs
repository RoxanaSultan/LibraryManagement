using AutoMapper;
using DomainModel.Entities;
using ServiceLayer.DTOs.Requests;

namespace TestServiceLayer;

public static class TestHelpers
{
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            // Minim necesar pentru serviciile tale
            cfg.CreateMap<BookCreateRequest, Book>()
               .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title));

            cfg.CreateMap<ReaderCreateRequest, Reader>()
               .ForMember(d => d.FirstName, opt => opt.MapFrom(s => s.FirstName))
               .ForMember(d => d.LastName, opt => opt.MapFrom(s => s.LastName));

            cfg.CreateMap<Loan, ServiceLayer.DTOs.Responses.LoanDetailsDto>();
            cfg.CreateMap<Reader, ServiceLayer.DTOs.Responses.ReaderDetailsDto>();
            cfg.CreateMap<Book, ServiceLayer.DTOs.Responses.BookListDto>();
        });

        config.AssertConfigurationIsValid();
        return config.CreateMapper();
    }
}