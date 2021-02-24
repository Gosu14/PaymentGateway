using System.Text.RegularExpressions;
using AutoMapper;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Api.Dto;

namespace PaymentGateway.Api.Mapping
{
    public class PaymentDemandMapperProfile : Profile
    {
        public PaymentDemandMapperProfile()
        {
            this.CreateMap<PaymentDemandDto, PaymentDemand>().ReverseMap();

            this.CreateMap<PaymentMethodDto, PaymentMethod>()
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => RemoveWhiteSpaces(src.CardBrand)))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => RemoveWhiteSpaces(src.CardCountry)))
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => RemoveWhiteSpaces(src.CardNumber)))
                .ForMember(dest => dest.ExpiryMonth, opt => opt.MapFrom(src => RemoveWhiteSpaces(src.CardExpiryMonth)))
                .ForMember(dest => dest.ExpiryYear, opt => opt.MapFrom(src => RemoveWhiteSpaces(src.CardExpiryYear)))
                .ForMember(dest => dest.Cvv, opt => opt.MapFrom(src => RemoveWhiteSpaces(src.CardCvv)))
                .ReverseMap();

            this.CreateMap<PaymentConfirmation, PaymentConfirmationDto>().ReverseMap();
        }

        private static string RemoveWhiteSpaces(string str) => Regex.Replace(str, @"\s+", "");
    }
}
