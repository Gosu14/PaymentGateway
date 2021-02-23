using System.Threading.Tasks;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Interfaces;

namespace PaymentGateway.Application.Services
{
    public class AcquiringBankGateway : IAcquiringBankGateway
    {
        public async Task<PaymentConfirmation> ProcessPaymentAsync(PaymentDemand paymentDemand)
        {
            await Task.Delay(5);
            return PaymentConfirmation.FromPaymentDemand(paymentDemand, "CONFIRMED");
        }
    }
}
