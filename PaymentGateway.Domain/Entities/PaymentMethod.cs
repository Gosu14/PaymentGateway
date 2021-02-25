namespace PaymentGateway.Domain.Entities
{
    public class PaymentMethod
    {
        public string Brand { get; set; }
        public string Country { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Number { get; set; }
        public string Cvv { get; set; }
    }
}
