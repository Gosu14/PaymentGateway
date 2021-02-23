using FluentValidation;
using PaymentGateway.Domain.Entities;
using System.Linq;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Domain.Constants;

namespace PaymentGateway.Application.Commands
{
    public class PaymentRequestValidator : AbstractValidator<PaymentDemand>
    {
        public PaymentRequestValidator(IDateService dateService)
        {
            this.RuleFor(v => v.Amount)
                .ExclusiveBetween(0, 1000000000).WithMessage("Amount must be between 0 and 1000000000");

            this.RuleFor(v => v.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency format must be 3 letters (ISO 4217)")
                .Must(v => PaymentDataConstants.PaymentCurrencyManaged.Contains(v.ToUpper())).WithMessage("Currency not managed");

            this.RuleFor(v => v.PaymentMethod.Cvv)
                .InclusiveBetween(0, 9999).WithMessage("Card CVC must be between 0 and 9999 included");

            this.RuleFor(v => v.PaymentMethod.Brand)
                .NotEmpty().WithMessage("Card Brand is required")
                .Must(v => PaymentDataConstants.CardBrandManaged.Contains(v.ToUpper())).WithMessage("Card type not managed");

            this.RuleFor(v => v.PaymentMethod.Country)
                .NotEmpty().WithMessage("Card country is required")
                .Length(2).WithMessage("Country code must be 2 letters (ISO 3166-1 alpha-2)")
                .Must(v => PaymentDataConstants.CountryCardProviderManaged.Contains(v.ToUpper())).WithMessage("Card from this country is not managed");

            this.RuleFor(v => v.PaymentMethod.ExpiryYear)
                .Must(v => v >= dateService.CurrentDateTime.Date.Year).WithMessage("Card is expired");

            this.RuleFor(v => v.PaymentMethod.ExpiryMonth)
                .InclusiveBetween(1, 12).WithMessage("Card expiry month should be between 1-12 included")
                .GreaterThanOrEqualTo(dateService.CurrentDateTime.Date.Month).WithMessage("Card is expired")
                .When(v => v.PaymentMethod.ExpiryYear == dateService.CurrentDateTime.Date.Year);

            this.RuleFor(v => v.PaymentMethod.Number).CreditCard().WithMessage("Invalid Card Number");
        }
    }
}
