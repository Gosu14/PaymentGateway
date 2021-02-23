using System;
using AutoMapper;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Api.Dto;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Api.Mapping
{
    public class PaymentDemandMapperProfile : Profile
    {
        public PaymentDemandMapperProfile()
        {
            this.CreateMap<PaymentDemandDto, PaymentDemand>().ReverseMap();

            this.CreateMap<PaymentMethodDto, PaymentMethod>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => GetPaymentMethodType(src.PaymentType)))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.CardBrand))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.CardCountry))
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.CardNumber))
                .ForMember(dest => dest.ExpiryMonth, opt => opt.MapFrom(src => src.CardExpiryMonth))
                .ForMember(dest => dest.ExpiryYear, opt => opt.MapFrom(src => src.CardExpiryYear))
                .ForMember(dest => dest.Cvv, opt => opt.MapFrom(src => src.CardCvv))
                .ReverseMap()
                .ForPath(s => s.PaymentType, opt => opt.MapFrom(src => nameof(PaymentMethodType.card)));

            this.CreateMap<PaymentConfirmation, PaymentConfirmationDto>().ReverseMap();
        }

        private static PaymentMethodType GetPaymentMethodType(string type) => Enum.TryParse(type, true, out PaymentMethodType typeToReturn) ? typeToReturn : PaymentMethodType.Unknown;
    }
}
