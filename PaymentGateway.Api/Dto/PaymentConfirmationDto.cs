using System;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Api.Dto
{
    public class PaymentConfirmationDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public PaymentMethodType Type { get; set; }
        public string CardBrand { get; set; }
        public string CardCountry { get; set; }
        public int CardExpiryYear { get; set; }
        public string Last4 { get; set; }
    }
}
