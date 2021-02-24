
namespace PaymentGateway.Api.Dto
{
    public class PaymentDemandDto
    {
        public long Amount { get; set; }
        public string Currency { get; set; }
        public PaymentMethodDto PaymentMethod { get; set; }
    }

    public class PaymentMethodDto
    {
        public string CardBrand { get; set; }
        public string CardCountry { get; set; }
        public int CardExpiryMonth { get; set; }
        public int CardExpiryYear { get; set; }
        public string CardNumber { get; set; }
        public string CardCvv { get; set; }
    }
}
