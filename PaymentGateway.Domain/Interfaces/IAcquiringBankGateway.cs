using System.Threading.Tasks;
using PaymentGateway.Domain.Entities;


namespace PaymentGateway.Domain.Interfaces
{
    public interface IAcquiringBankGateway
    {
        public Task<string> ProcessPaymentAsync(PaymentDemand paymentDemand);
    }
}
