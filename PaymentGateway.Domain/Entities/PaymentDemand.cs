
namespace PaymentGateway.Domain.Entities
{
    public class PaymentDemand
    {
        public long Amount { get; set; }
        public string Currency { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
