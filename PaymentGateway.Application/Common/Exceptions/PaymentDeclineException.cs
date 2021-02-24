using System;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Common.Exceptions
{
    public class PaymentDeclineException : Exception
    {
        public PaymentConfirmation PaymentDeclined { get; set; }

        public PaymentDeclineException(PaymentConfirmation paymentConfirmation) : base("Payment has been declined by Card issuer.") => this.PaymentDeclined = paymentConfirmation;
    }
}
