namespace PaymentGateway.Api.Dto
{
    public class PaymentDemandDto
    {
        public int Amount { get; set; }
        public string Currency { get; set; }
        public PaymentMethodDto PaymentMethod { get; set; }
    }

    public class PaymentMethodDto
    {
        public string PaymentType { get; set; }
        public string CardBrand { get; set; }
        public string CardCountry { get; set; }
        public string CardExpiryMonth { get; set; }
        public string CardExpiryYear { get; set; }
        public string CardNumber { get; set; }
        public string CardCvv { get; set; }
    }
}
