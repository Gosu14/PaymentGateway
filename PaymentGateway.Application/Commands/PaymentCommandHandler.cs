using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Common.Exceptions;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Domain.Interfaces;
using ValidationException = PaymentGateway.Application.Common.Exceptions.ValidationException;

namespace PaymentGateway.Application.Commands
{
    public class PaymentCommandHandler : ICommandHandler<PaymentDemand, PaymentConfirmation>
    {
        private readonly IValidator<PaymentDemand> paymentRequestValidator;
        private readonly IAcquiringBankGateway acquiringBankGateway;
        private readonly IApplicationDbContext dbContext;
        private readonly ILogger<PaymentCommandHandler> logger;

        public PaymentCommandHandler(IValidator<PaymentDemand> paymentRequestValidator, IAcquiringBankGateway acquiringBankGateway, IApplicationDbContext dbContext, ILogger<PaymentCommandHandler> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.paymentRequestValidator = paymentRequestValidator ?? throw new ArgumentNullException(nameof(paymentRequestValidator));
            this.acquiringBankGateway = acquiringBankGateway ?? throw new ArgumentNullException(nameof(acquiringBankGateway));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handle the Payment demand request
        /// </summary>
        /// <param name="command">The PaymentDemand to process</param>
        /// <returns></returns>
        public async Task<PaymentConfirmation> ExecuteAsync(PaymentDemand command)
        {
            this.Validate(command);
            var paymentConfirmation = await this.acquiringBankGateway.ProcessPaymentAsync(command);
            var saveResult = this.SavePaymentConfirmation(paymentConfirmation);
            this.ThrowExceptionIfPaymentDeclined(paymentConfirmation);
            return paymentConfirmation;
        }

        /// <summary>
        /// Validate the inputs within the PaymentDemand
        /// </summary>
        /// <param name="toValidate">PaymentDemand to validate</param>
        private void Validate(PaymentDemand toValidate)
        {
            ValidationResult validationResult;

            try
            {
                validationResult = this.paymentRequestValidator.Validate(toValidate);
            }
            catch
            {
                this.logger.LogInformation("PaymentDemand inputted had a wrong format. Throwing a ValidationException");
                throw new ValidationException(new[] { new ValidationFailure("$", "Bad request formatting, please check the JSON payload format.") });
            }

            if (!validationResult.IsValid)
            {
                this.logger.LogInformation("PaymentDemand inputted had validations errors. Throwing a ValidationException");
                throw new ValidationException(validationResult.Errors);
            }
        }

        /// <summary>
        /// Save PaymentConfirmation in DB
        /// </summary>
        /// <param name="toSave">PaymentDemand to save</param>
        /// <returns></returns>
        private async Task<int> SavePaymentConfirmation(PaymentConfirmation toSave)
        {
            await this.dbContext.PaymentConfirmations.AddAsync(toSave);
            return await this.dbContext.SaveChangesAsync(new CancellationToken());
        }

        /// <summary>
        /// Throws Exception if PaymentConfirmation has a Status of PAYMENT_DECLINED
        /// </summary>
        /// <param name="toCheck">PaymentConfirmation to check</param>
        private void ThrowExceptionIfPaymentDeclined(PaymentConfirmation toCheck)
        {
            if (toCheck.Status.StartsWith("PAYMENT_DECLINED", StringComparison.InvariantCulture))
            {
                this.logger.LogInformation("Payment had been declined for Payment Confirmation {Id}. Throwing a PaymentDeclineException", toCheck.Id);
                throw new PaymentDeclineException(toCheck);
            }
        }
    }
}
