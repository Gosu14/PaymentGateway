using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Domain.Interfaces;

namespace PaymentGateway.Application.Commands
{
    public class PaymentCommandHandler : ICommandHandler<PaymentDemand, PaymentConfirmation>
    {
        private readonly IValidator<PaymentDemand> paymentRequestValidator;
        private readonly IAcquiringBankGateway acquiringBankGateway;
        private readonly IApplicationDbContext dbContext;

        public PaymentCommandHandler(IValidator<PaymentDemand> paymentRequestValidator, IAcquiringBankGateway acquiringBankGateway, IApplicationDbContext dbContext)
        {
            this.paymentRequestValidator = paymentRequestValidator;
            this.acquiringBankGateway = acquiringBankGateway;
            this.dbContext = dbContext;
        }

        public async Task<PaymentConfirmation> ExecuteAsync(PaymentDemand command)
        {
            this.Validate(command);
            var paymentConfirmation = await this.acquiringBankGateway.ProcessPaymentAsync(command);
            await this.dbContext.PaymentConfirmations.AddAsync(paymentConfirmation);
            await this.dbContext.SaveChangesAsync(new CancellationToken());
            return paymentConfirmation;
        }

        private void Validate(PaymentDemand toValidate)
        {
            var validationResult = this.paymentRequestValidator.Validate(toValidate);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}
