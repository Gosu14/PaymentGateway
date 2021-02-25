using System;
using System.Linq;
using FluentValidation;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Domain.Constants;

namespace PaymentGateway.Application.Commands
{
    public class PaymentRequestValidator : AbstractValidator<PaymentDemand>
    {
        public PaymentRequestValidator(IDateService dateService)
        {
            if (dateService == null)
            {
                throw new ArgumentNullException(nameof(dateService));
            }

            this.RuleFor(v => v.Amount)
                .ExclusiveBetween(0, 1000000).WithMessage("Amount must be between 0 and 1000000");

            this.RuleFor(v => v.Currency)
                .NotNull()
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency format must be 3 letters (ISO 4217)")
                .Must(v => PaymentDataConstants.PaymentCurrencyManaged.Contains(v?.ToUpperInvariant())).WithMessage("Invalid currency");

            this.RuleFor(v => v.PaymentMethod.Cvv)
                .MinimumLength(3)
                .MaximumLength(4)
                .WithMessage("Card CVC must be between 0 and 9999 included");

            this.RuleFor(v => v.PaymentMethod.Brand)
                .NotNull()
                .NotEmpty().WithMessage("Card Brand is required");

            this.RuleFor(v => v.PaymentMethod.Country)
                .Must(v => PaymentDataConstants.CountryCardProviderManaged.Contains(v?.ToUpperInvariant()))
                .WithMessage("Invalid country")
                .When(v => !string.IsNullOrEmpty(v.PaymentMethod.Country))
                .NotNull()
                .NotEmpty().WithMessage("Card country is required")
                .Length(2).WithMessage("Country code must be 2 letters (ISO 3166-1 alpha-2)");

            this.RuleFor(v => v.PaymentMethod.ExpiryYear)
                .Must(v => v >= dateService.CurrentDateTime.Date.Year).WithMessage("Card is expired");

            this.RuleFor(v => v.PaymentMethod.ExpiryMonth)
                .InclusiveBetween(1, 12).WithMessage("Card expiry month should be between 1-12 included")
                .GreaterThanOrEqualTo(dateService.CurrentDateTime.Date.Month).WithMessage("Card is expired")
                .When(v => v.PaymentMethod.ExpiryYear == dateService.CurrentDateTime.Date.Year);

            this.RuleFor(v => v.PaymentMethod.Number).CreditCard().WithMessage("Invalid card number");
        }
    }
}
