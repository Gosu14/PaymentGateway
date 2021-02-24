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

            if (IsMasterCard(paymentDemand.PaymentMethod))
            {
                return PaymentConfirmation.FromPaymentDemand(paymentDemand,
                    GetMastercardCheckConfirmation(paymentDemand.PaymentMethod));
            }
            else if (IsVisa(paymentDemand.PaymentMethod))
            {
                return PaymentConfirmation.FromPaymentDemand(paymentDemand,
                    GetVisaCheckConfirmation(paymentDemand.PaymentMethod));
            }

            return PaymentConfirmation.FromPaymentDemand(paymentDemand, PaymentConfirmationCode.PaymentAccepted);
        }

        private static bool IsMasterCard(PaymentMethod card) => card.Brand.ToUpperInvariant().Trim() is "MASTERCARD";
        private static bool IsVisa(PaymentMethod card) => card.Brand.ToUpperInvariant().Trim() is "VISA";

        private static string GetMastercardCheckConfirmation(PaymentMethod cardDetail)
        {
            if (RemoveWhiteSpaces(cardDetail.Number) is not "5555555555554444")
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidNumber;
            }else if (cardDetail.ExpiryMonth != 3)
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryMonth;
            }else if(cardDetail.ExpiryYear != 2030)
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryYear;
            }else if (cardDetail.Cvv != 737)
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidCvv;
            }

            return PaymentConfirmationCode.PaymentAccepted;
        }

        private static string GetVisaCheckConfirmation(PaymentMethod cardDetail)
        {
            if (RemoveWhiteSpaces(cardDetail.Number) is not "4977949494949497")
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidNumber;
            }
            else if (cardDetail.ExpiryMonth != 3)
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryMonth;
            }
            else if (cardDetail.ExpiryYear != 2030)
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryYear;
            }
            else if (cardDetail.Cvv != 737)
            {
                return PaymentConfirmationCode.PaymentDeclinedCardInvalidCvv;
            }

            return PaymentConfirmationCode.PaymentAccepted;
        }

        private static string RemoveWhiteSpaces(string str) => Regex.Replace(str, @"\s+", "");

    }
}
