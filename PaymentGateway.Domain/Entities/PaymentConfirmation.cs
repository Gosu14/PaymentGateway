using System;

namespace PaymentGateway.Domain.Entities
{
    public class PaymentConfirmation
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public long Amount { get; set; }
        public string Currency { get; set; }
        public string CardBrand { get; set; }
        public string CardCountry { get; set; }
        public int CardExpiryYear { get; set; }
        public string Last4 { get; set; }

        public static PaymentConfirmation FromPaymentDemand(PaymentDemand demand, string status) => new()
        {
            Id = Guid.NewGuid(),
            Status = status,
            Amount = demand.Amount,
            Currency = demand.Currency,
            CardBrand = demand.PaymentMethod.Brand,
            CardCountry = demand.PaymentMethod.Country,
            CardExpiryYear = demand.PaymentMethod.ExpiryYear,
            Last4 = demand.PaymentMethod.Number[^4..]
        };
    }

    public static class PaymentConfirmationCode
    {
        public const string PaymentAccepted = "PAYMENT_ACCEPTED";
        public const string PaymentDeclinedCardNotSupported = "PAYMENT_DECLINED_CARD_NOT_SUPPORTED";
        public const string PaymentDeclinedCardInvalidNumber = "PAYMENT_DECLINED_CARD_INVALID_NUMBER";
        public const string PaymentDeclinedCardInvalidCvv = "PAYMENT_DECLINED_CARD_INVALID_CVV";
        public const string PaymentDeclinedCardInvalidExpiryYear = "PAYMENT_DECLINED_CARD_INVALID_EXPIRY_YEAR";
        public const string PaymentDeclinedCardInvalidExpiryMonth = "PAYMENT_DECLINED_CARD_INVALID_EXPIRY_MONTH";
        public const string PaymentDeclinedCardStolen = "PAYMENT_DECLINED_CARD_STOLEN";
        public const string PaymentDeclinedInsufficientFunds = "PAYMENT_DECLINED_INSUFFICIENT_FUNDS";
    }
}
