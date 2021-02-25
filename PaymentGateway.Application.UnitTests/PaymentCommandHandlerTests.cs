using Xunit;
using System.Threading.Tasks;
using PaymentGateway.Application.Commands;
using Moq;
using FluentValidation;
using System.Collections.Generic;
using FluentValidation.Results;
using PaymentGateway.Domain.Interfaces;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.UnitTests
{
    public class PaymentCommandHandlerTests
    {
        public Mock<IValidator<PaymentDemand>> ValidatorMock { get; } = new();

        public Mock<IAcquiringBankGateway> AcquiringBankGatewayMock { get; } = new(MockBehavior.Strict);

        public Mock<IApplicationDbContext> DbContext { get; } = new(MockBehavior.Strict);

        public Mock<ILogger<PaymentCommandHandler>> Logger { get; } = new();

        public PaymentDemand ValidPaymentDemand { get; } = new PaymentDemand()
        {
            Amount = 500,
            Currency = "usd",
            PaymentMethod = new PaymentMethod()
            {
                Brand = "visa",
                Country = "fr",
                Number = "4977 9494 9494 9497",
                Cvv = "737",
                ExpiryYear = 03,
                ExpiryMonth = 2030
            }
        };

        [Fact]
        public async Task ShouldThrowValidationException()
        {
            //Arrange
            var validationResultMock =
                new ValidationResult(new List<ValidationFailure>() { new("test", "test", null) });

            this.ValidatorMock.Setup(x => x.Validate(It.IsAny<PaymentDemand>())).Returns(validationResultMock);

            var paymentCommandHandler = new PaymentCommandHandler(this.ValidatorMock.Object, this.AcquiringBankGatewayMock.Object, this.DbContext.Object, this.Logger.Object);

            //Act - Assert
            await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() => paymentCommandHandler.ExecuteAsync(new PaymentDemand()));
        }

        [Theory]
        [InlineData(PaymentConfirmationCode.PaymentDeclinedCardInvalidCvv)]
        [InlineData(PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryMonth)]
        [InlineData(PaymentConfirmationCode.PaymentDeclinedCardInvalidExpiryYear)]
        [InlineData(PaymentConfirmationCode.PaymentDeclinedCardInvalidNumber)]
        [InlineData(PaymentConfirmationCode.PaymentDeclinedCardNotSupported)]
        [InlineData(PaymentConfirmationCode.PaymentDeclinedCardStolen)]
        [InlineData(PaymentConfirmationCode.PaymentDeclinedInsufficientFunds)]
        public async Task ShouldThrowPaymentDeclinedException(string paymentDeclinedCode)
        {
            //Arrange
            this.ValidatorMock.Setup(x => x.Validate(It.IsAny<PaymentDemand>())).Returns(new ValidationResult());

            this.AcquiringBankGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentDemand>()))
                .ReturnsAsync(paymentDeclinedCode);

            this.DbContext.Setup(dbc => dbc.PaymentConfirmations.AddAsync(It.IsAny<PaymentConfirmation>(), new CancellationToken()))
                .ReturnsAsync(It.IsAny<EntityEntry<PaymentConfirmation>>());

            this.DbContext.Setup(dbc => dbc.SaveChangesAsync(new CancellationToken()))
                .ReturnsAsync(It.IsAny<int>());

            var paymentCommandHandler = new PaymentCommandHandler(this.ValidatorMock.Object, this.AcquiringBankGatewayMock.Object, this.DbContext.Object, this.Logger.Object);

            //Act - Assert
            await Assert.ThrowsAsync<Common.Exceptions.PaymentDeclineException>(() => paymentCommandHandler.ExecuteAsync(this.ValidPaymentDemand));
        }

        [Fact]
        public async Task ShouldReturnPaymentConfirmationWhenSuccessful()
        {
            //Arrange
            this.ValidatorMock.Setup(x => x.Validate(It.IsAny<PaymentDemand>())).Returns(new ValidationResult());

            this.AcquiringBankGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentDemand>()))
                .ReturnsAsync(PaymentConfirmationCode.PaymentAccepted);

            this.DbContext.Setup(dbc => dbc.PaymentConfirmations.AddAsync(It.IsAny<PaymentConfirmation>(), new CancellationToken()))
                .ReturnsAsync(It.IsAny<EntityEntry<PaymentConfirmation>>());

            this.DbContext.Setup(dbc => dbc.SaveChangesAsync(new CancellationToken()))
                .ReturnsAsync(It.IsAny<int>());

            var paymentCommandHandler = new PaymentCommandHandler(this.ValidatorMock.Object, this.AcquiringBankGatewayMock.Object, this.DbContext.Object, this.Logger.Object);

            //Act
            var result = await paymentCommandHandler.ExecuteAsync(this.ValidPaymentDemand);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<PaymentConfirmation>(result);
        }

        [Fact]
        public async Task ShouldCallValidatorMethod()
        {
            //Arrange
            this.ValidatorMock.Setup(x => x.Validate(It.IsAny<PaymentDemand>())).Returns(new ValidationResult());

            this.AcquiringBankGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentDemand>()))
                .ReturnsAsync(PaymentConfirmationCode.PaymentAccepted);

            this.DbContext.Setup(dbc => dbc.PaymentConfirmations.AddAsync(It.IsAny<PaymentConfirmation>(), new CancellationToken()))
                .ReturnsAsync(It.IsAny<EntityEntry<PaymentConfirmation>>());

            this.DbContext.Setup(dbc => dbc.SaveChangesAsync(new CancellationToken()))
                .ReturnsAsync(It.IsAny<int>());

            var paymentCommandHandler = new PaymentCommandHandler(this.ValidatorMock.Object, this.AcquiringBankGatewayMock.Object, this.DbContext.Object, this.Logger.Object);

            //Act
            var result = await paymentCommandHandler.ExecuteAsync(this.ValidPaymentDemand);

            //Assert
            this.ValidatorMock.Verify(x => x.Validate(It.IsAny<PaymentDemand>()), Times.Once);
        }

        [Fact]
        public async Task ShouldCallProcessPaymentAsyncFromAcquiringBankGateway()
        {
            //Arrange
            this.ValidatorMock.Setup(x => x.Validate(It.IsAny<PaymentDemand>())).Returns(new ValidationResult());

            this.AcquiringBankGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentDemand>()))
                .ReturnsAsync(PaymentConfirmationCode.PaymentAccepted);

            this.DbContext.Setup(dbc => dbc.PaymentConfirmations.AddAsync(It.IsAny<PaymentConfirmation>(), new CancellationToken()))
                .ReturnsAsync(It.IsAny<EntityEntry<PaymentConfirmation>>());

            this.DbContext.Setup(dbc => dbc.SaveChangesAsync(new CancellationToken()))
                .ReturnsAsync(It.IsAny<int>());

            var paymentDemand = this.ValidPaymentDemand;

            var paymentCommandHandler = new PaymentCommandHandler(this.ValidatorMock.Object, this.AcquiringBankGatewayMock.Object, this.DbContext.Object, this.Logger.Object);

            //Act
            var result = await paymentCommandHandler.ExecuteAsync(paymentDemand);

            //Assert
            this.AcquiringBankGatewayMock.Verify(x => x.ProcessPaymentAsync(paymentDemand), Times.Once);
        }
    }
}
