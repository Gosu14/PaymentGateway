using Xunit;
using System.Threading.Tasks;
using PaymentGateway.Application.Commands;
using Moq;
using FluentValidation;
using System.Collections.Generic;
using FluentValidation.Results;
using PaymentGateway.Domain.Interfaces;
using System;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.UnitTests
{
    public class PaymentCommandHandlerTests
    {
        public Mock<IValidator<PaymentDemand>> ValidatorMock { get; } =
            new Mock<IValidator<PaymentDemand>>(MockBehavior.Strict);

        public Mock<IAcquiringBankGateway> AcquiringBankGatewayMock { get; } =
            new Mock<IAcquiringBankGateway>(MockBehavior.Strict);

        public Mock<IApplicationDbContext> dbContext { get; } =
            new Mock<IApplicationDbContext>(MockBehavior.Strict);

        [Fact]
        public async Task ShouldThrowValidationException()
        {
            //Arrange
            var validationResultMock =
                new ValidationResult(new List<ValidationFailure>() { new ValidationFailure("test", "test", null) });
            this.ValidatorMock.Setup(x => x.Validate(It.IsAny<PaymentDemand>())).Returns(validationResultMock);

            var paymentCommandHandler = new PaymentCommandHandler(this.ValidatorMock.Object, this.AcquiringBankGatewayMock.Object, this.dbContext.Object);

            //Act - Assert
            await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() => paymentCommandHandler.ExecuteAsync(new PaymentDemand()));
        }

        [Fact]
        public async Task ShouldReturnPaymentConfirmationWhenSuccessful()
        {
            //Arrange
            this.ValidatorMock.Setup(x => x.Validate(It.IsAny<PaymentDemand>())).Returns(new ValidationResult());

            var paymentConfirmation = new PaymentConfirmation() { Id = Guid.NewGuid(), Status = "Confirmed" };
            this.AcquiringBankGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentDemand>()))
                .ReturnsAsync(paymentConfirmation);

            this.dbContext.Setup(dbc => dbc.PaymentConfirmations.AddAsync(It.IsAny<PaymentConfirmation>(), new CancellationToken()))
                .ReturnsAsync(It.IsAny<EntityEntry<PaymentConfirmation>>());

            this.dbContext.Setup(dbc => dbc.SaveChangesAsync(new CancellationToken()))
                .ReturnsAsync(It.IsAny<int>());

            var paymentCommandHandler = new PaymentCommandHandler(this.ValidatorMock.Object, this.AcquiringBankGatewayMock.Object, this.dbContext.Object);

            //Act
            var result = await paymentCommandHandler.ExecuteAsync(new PaymentDemand());

            //Assert
            Assert.NotNull(result);
            Assert.IsType<PaymentConfirmation>(result);
        }

        [Fact]
        public async Task ShouldCallValidatorMethod()
        {
            //Arrange
            this.ValidatorMock.Setup(x => x.Validate(It.IsAny<PaymentDemand>())).Returns(new ValidationResult());

            var paymentDemand = new PaymentDemand();

            var paymentConfirmation = new PaymentConfirmation() { Id = Guid.NewGuid(), Status = "Confirmed" };
            this.AcquiringBankGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentDemand>()))
                .ReturnsAsync(paymentConfirmation);

            this.dbContext.Setup(dbc => dbc.PaymentConfirmations.AddAsync(It.IsAny<PaymentConfirmation>(), new CancellationToken()))
                .ReturnsAsync(It.IsAny<EntityEntry<PaymentConfirmation>>());

            this.dbContext.Setup(dbc => dbc.SaveChangesAsync(new CancellationToken()))
                .ReturnsAsync(It.IsAny<int>());

            var paymentCommandHandler = new PaymentCommandHandler(this.ValidatorMock.Object, this.AcquiringBankGatewayMock.Object, this.dbContext.Object);

            //Act
            var result = await paymentCommandHandler.ExecuteAsync(paymentDemand);

            //Assert
            this.ValidatorMock.Verify(x => x.Validate(It.IsAny<PaymentDemand>()), Times.Once);
        }

        [Fact]
        public async Task ShouldCallProcessPaymentAsyncFromAcquiringBankGateway()
        {
            //Arrange
            var validationResultMock =
                new ValidationResult(new List<ValidationFailure>() { new ValidationFailure("test", "test", null) });
            this.ValidatorMock.Setup(x => x.Validate(It.IsAny<PaymentDemand>())).Returns(new ValidationResult());

            var paymentDemand = new PaymentDemand();

            var paymentConfirmation = new PaymentConfirmation() { Id = Guid.NewGuid(), Status = "Confirmed" };
            this.AcquiringBankGatewayMock.Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentDemand>()))
                .ReturnsAsync(paymentConfirmation);

            this.dbContext.Setup(dbc => dbc.PaymentConfirmations.AddAsync(It.IsAny<PaymentConfirmation>(), new CancellationToken()))
                .ReturnsAsync(It.IsAny<EntityEntry<PaymentConfirmation>>());

            this.dbContext.Setup(dbc => dbc.SaveChangesAsync(new CancellationToken()))
                .ReturnsAsync(It.IsAny<int>());

            var paymentCommandHandler = new PaymentCommandHandler(this.ValidatorMock.Object, this.AcquiringBankGatewayMock.Object, this.dbContext.Object);

            //Act
            var result = await paymentCommandHandler.ExecuteAsync(paymentDemand);

            //Assert
            this.AcquiringBankGatewayMock.Verify(x => x.ProcessPaymentAsync(paymentDemand), Times.Once);
        }
    }
}
