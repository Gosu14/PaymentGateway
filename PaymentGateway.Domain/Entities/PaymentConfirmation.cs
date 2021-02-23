using System;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Domain.Entities
{
    public class PaymentConfirmation
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

        public static PaymentConfirmation FromPaymentDemand(PaymentDemand demand, string status) => new PaymentConfirmation()
        {
            Id = Guid.NewGuid(),
            Status = status,
            Amount = demand.Amount,
            Currency = demand.Currency,
            Type = demand.PaymentMethod.Type,
            CardBrand = demand.PaymentMethod.Brand,
            CardCountry = demand.PaymentMethod.Country,
            CardExpiryYear = demand.PaymentMethod.ExpiryYear,
            Last4 = demand.PaymentMethod.Number[^4..]
        };
    }
}
