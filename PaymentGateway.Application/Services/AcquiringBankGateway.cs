using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Interfaces;

namespace PaymentGateway.Application.Services
{
    /// <summary>
    /// Class Mocking the Acquiring bank processing
    /// It will be used to mock different return code
    /// </summary>
    public class AcquiringBankGateway : IAcquiringBankGateway
    {
        /// <summary>
        /// Process the payment instruction
        /// </summary>
        /// <param name="paymentDemand">Payment demand containing the payment method</param>
        /// <returns></returns>
        public async Task<PaymentConfirmation> ProcessPaymentAsync(PaymentDemand paymentDemand)
        {
            //This class is a mock of the Acquiring bank processing
            //I will use it also to setup specific Tests cases to vary responses
            await Task.Delay(5);

            if (!(IsMasterCard(paymentDemand.PaymentMethod) || IsVisa(paymentDemand.PaymentMethod)))
            {
                return PaymentConfirmation.FromPaymentDemand(paymentDemand, PaymentConfirmationCode.PaymentDeclinedCardNotSupported);
            }

            if (paymentDemand.Amount > 1000000000)
            {
                return PaymentConfirmation.FromPaymentDemand(paymentDemand, PaymentConfirmationCode.PaymentDeclinedInsufficientFunds);
            }

            if (IsStolenCard(paymentDemand.PaymentMethod))
            {
                return PaymentConfirmation.FromPaymentDemand(paymentDemand, PaymentConfirmationCode.PaymentDeclinedCardStolen);
            }

            if (IsMasterCard(paymentDemand.PaymentMethod))
            {
                return PaymentConfirmation.FromPaymentDemand(paymentDemand,
                    GetMastercardCheckConfirmation(paymentDemand.PaymentMethod));
            }

            if (IsVisa(paymentDemand.PaymentMethod))
            {
                return PaymentConfirmation.FromPaymentDemand(paymentDemand,
                    GetVisaCheckConfirmation(paymentDemand.PaymentMethod));
            }

            return PaymentConfirmation.FromPaymentDemand(paymentDemand, PaymentConfirmationCode.PaymentAccepted);
        }

        private static bool IsMasterCard(PaymentMethod card) => RemoveWhiteSpaces(card.Brand.ToUpperInvariant()) is "MASTERCARD";

        private static bool IsVisa(PaymentMethod card) => RemoveWhiteSpaces(card.Brand.ToUpperInvariant()) is "VISA";

        private static string GetMastercardCheckConfirmation(PaymentMethod cardDetail)
        {
            if (RemoveWhiteSpaces(cardDetail.Number) is not "5555555555554444")
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidNumber;
            }
            if (cardDetail.ExpiryMonth != 3)
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryMonth;
            }
            if (cardDetail.ExpiryYear != 2030)
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryYear;
            }
            return RemoveWhiteSpaces(cardDetail.Cvv) != "737" ? PaymentConfirmationCode.PaymentDeclinedCardInvalidCvv : PaymentConfirmationCode.PaymentAccepted;
        }

        private static string GetVisaCheckConfirmation(PaymentMethod cardDetail)
        {
            if (RemoveWhiteSpaces(cardDetail.Number) is not "4977949494949497")
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidNumber;
            }
            if (cardDetail.ExpiryMonth != 3)
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryMonth;
            }
            if (cardDetail.ExpiryYear != 2030)
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryYear;
            }
            return RemoveWhiteSpaces(cardDetail.Cvv) != "737" ? PaymentConfirmationCode.PaymentDeclinedCardInvalidCvv : PaymentConfirmationCode.PaymentAccepted;
        }

        private static bool IsStolenCard(PaymentMethod cardDetail)
        {
            if (RemoveWhiteSpaces(cardDetail.Brand.ToUpperInvariant()) is "VISA"
                && RemoveWhiteSpaces(cardDetail.Number) is "4000020000000000"
                && cardDetail.ExpiryYear == 2030
                && cardDetail.ExpiryMonth == 03
                && RemoveWhiteSpaces(cardDetail.Cvv) is "737")
            {
                return true;
            }

            return false;
        }

        private static string RemoveWhiteSpaces(string str) => Regex.Replace(str, @"\s+", "");

    }
}
