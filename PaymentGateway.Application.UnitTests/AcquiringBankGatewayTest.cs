using System.Threading.Tasks;
using PaymentGateway.Application.Services;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Interfaces;
using Xunit;

namespace PaymentGateway.Application.UnitTests
{
    public class AcquiringBankGatewayTest
    {
        public PaymentDemand ValidPaymentVisa { get; } = new PaymentDemand
        {
            Amount = 500,
            Currency = "usd",
            PaymentMethod = new PaymentMethod()
            {
                Brand = "visa",
                Country = "fr",
                Cvv = "737",
                Number = "4977 9494 9494 9497",
                ExpiryYear = 2030,
                ExpiryMonth = 03
            }
        };

        public PaymentDemand ValidPaymentMastercard { get; } = new PaymentDemand
        {
            Amount = 500,
            Currency = "usd",
            PaymentMethod = new PaymentMethod()
            {
                Brand = "mastercard",
                Country = "GB",
                Cvv = "737",
                Number = "5555 5555 5555 4444",
                ExpiryYear = 2030,
                ExpiryMonth = 03
            }
        };

        public PaymentDemand PaymentWithStolenCard { get; } = new PaymentDemand
        {
            Amount = 500,
            Currency = "usd",
            PaymentMethod = new PaymentMethod()
            {
                Brand = "visa",
                Country = "US",
                Cvv = "737",
                Number = "4000 0200 0000 0000",
                ExpiryYear = 2030,
                ExpiryMonth = 03
            }
        };

        public IAcquiringBankGateway AcquiringBankGateway { get; } = new AcquiringBankGateway();

        [Fact]
        public async Task ShouldValidatePaymentWithValidVisaCard()
        {
            //Act
            var result = await this.AcquiringBankGateway.ProcessPaymentAsync(this.ValidPaymentVisa);

            //Assert
            Assert.Equal(PaymentConfirmationCode.PaymentAccepted, result);
        }

        [Fact]
        public async Task ShouldValidatePaymentWithValidMastercard()
        {
            //Act
            var result = await this.AcquiringBankGateway.ProcessPaymentAsync(this.ValidPaymentMastercard);

            //Assert
            Assert.Equal(PaymentConfirmationCode.PaymentAccepted, result);
        }

        [Fact]
        public async Task ShouldDeclinePaymentIfCardIsNotMastercardOrVisa()
        {
            //Arrange
            var unmanagedCard = this.ValidPaymentMastercard;
            unmanagedCard.PaymentMethod.Brand = "Unmanaged";

            //Act
            var result = await this.AcquiringBankGateway.ProcessPaymentAsync(unmanagedCard);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentConfirmationCode.PaymentDeclinedCardNotSupported, result);
        }

        [Fact]
        public async Task ShouldDeclinePaymentIfCvvIsInvalid()
        {
            //Arrange
            var unmanagedCard = this.ValidPaymentMastercard;
            unmanagedCard.PaymentMethod.Cvv = "000";

            //Act
            var result = await this.AcquiringBankGateway.ProcessPaymentAsync(unmanagedCard);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentConfirmationCode.PaymentDeclinedCardInvalidCvv, result);
        }

        [Fact]
        public async Task ShouldDeclinePaymentIfCardExpiryMonthIsInvalid()
        {
            //Arrange
            var unmanagedCard = this.ValidPaymentMastercard;
            unmanagedCard.PaymentMethod.ExpiryMonth = 11;

            //Act
            var result = await this.AcquiringBankGateway.ProcessPaymentAsync(unmanagedCard);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryMonth, result);
        }

        [Fact]
        public async Task ShouldDeclinePaymentIfCardExpiryYearIsInvalid()
        {
            //Arrange
            var unmanagedCard = this.ValidPaymentMastercard;
            unmanagedCard.PaymentMethod.ExpiryYear = 2025;

            //Act
            var result = await this.AcquiringBankGateway.ProcessPaymentAsync(unmanagedCard);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryYear, result);
        }

        [Fact]
        public async Task ShouldDeclinePaymentIfCardIsStolen()
        {
            //Act
            var result = await this.AcquiringBankGateway.ProcessPaymentAsync(this.PaymentWithStolenCard);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentConfirmationCode.PaymentDeclinedCardStolen, result);
        }

        [Fact]
        public async Task ShouldDeclinePaymentIfInsufficientFunds()
        {
            //Arrange
            var unmanagedCard = this.ValidPaymentMastercard;
            unmanagedCard.Amount = 1000001;

            //Act
            var result = await this.AcquiringBankGateway.ProcessPaymentAsync(unmanagedCard);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentConfirmationCode.PaymentDeclinedInsufficientFunds, result);
        }
    }
}
