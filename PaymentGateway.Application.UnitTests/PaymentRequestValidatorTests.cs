using PaymentGateway.Application.Common.Interfaces;
using Moq;
using Xunit;
using PaymentGateway.Application.Commands;
using PaymentGateway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using PaymentGateway.Domain.Constants;
using FluentValidation.TestHelper;

namespace PaymentGateway.Application.UnitTests
{
    public class PaymentRequestValidatorTests
    {
        public Mock<IDateService> DateServiceMock { get; } = new Mock<IDateService>(MockBehavior.Strict);
        public PaymentDemand ValidPaymentDemand { get; } = new PaymentDemand
        {
            Amount = 500,
            Currency = "usd",
            PaymentMethod = new PaymentMethod()
            {
                Brand = "visa",
                Country = "fr",
                Cvv = 737,
                Number = "4977 9494 9494 9497",
                ExpiryYear = 2030,
                ExpiryMonth = 03
            }
        };

        [Fact]
        public void ShouldNotHaveErrorWhenObjectIsCorrect()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);

            //Act
            var result = validator.TestValidate(this.ValidPaymentDemand);

            //Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void ShouldHaveErrorWhenExpiryYearIsInThePast()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = this.ValidPaymentDemand;
            invalidPaymentDemand.PaymentMethod.ExpiryYear = 2019;

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldHaveValidationErrorFor(paymentDemand => paymentDemand.PaymentMethod.ExpiryYear);
        }

        [Fact]
        public void ShouldHaveErrorWhenDateIsInThePast()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 02, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = this.ValidPaymentDemand;
            invalidPaymentDemand.PaymentMethod.ExpiryYear = 2020;
            invalidPaymentDemand.PaymentMethod.ExpiryMonth = 01;

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldHaveValidationErrorFor(paymentDemand => paymentDemand.PaymentMethod.ExpiryMonth);
        }

        [Fact]
        public void ShouldHaveErrorWhenCurrencyIsInvalid()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = this.ValidPaymentDemand;
            invalidPaymentDemand.Currency = "us";

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldHaveValidationErrorFor(paymentDemand => paymentDemand.Currency);
        }

        public static IEnumerable<object[]> GetValidCurrencies() => PaymentDataConstants.PaymentCurrencyManaged.Select(ccy => new object[] { ccy });

        [Theory]
        [MemberData(nameof(GetValidCurrencies))]
        public void ShouldNotHaveErrorWhenCurrenciesIsManaged(string currency)
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var paymentDemand = this.ValidPaymentDemand;
            paymentDemand.Currency = currency;

            //Act
            var result = validator.TestValidate(paymentDemand);

            //Assert
            result.ShouldNotHaveAnyValidationErrors();
        }


        [Fact]
        public void ShouldHaveErrorWhenCountryIssuingCardIsInvalid()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = this.ValidPaymentDemand;
            invalidPaymentDemand.PaymentMethod.Country = "Country";

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldHaveValidationErrorFor(paymentDemand => paymentDemand.PaymentMethod.Country);
        }

        public static IEnumerable<object[]> GetValidCardCountry() => PaymentDataConstants.CountryCardProviderManaged.Select(country => new object[] { country });

        [Theory]
        [MemberData(nameof(GetValidCardCountry))]
        public void ShouldNotHaveErrorWhenCountryIssuingCardIsManaged(string country)
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var paymentDemand = this.ValidPaymentDemand;
            paymentDemand.PaymentMethod.Country = country;

            //Act
            var result = validator.TestValidate(paymentDemand);

            //Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void ShouldHaveErrorWhenCvvIsInvalid()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = this.ValidPaymentDemand;
            invalidPaymentDemand.PaymentMethod.Cvv = 55555;

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldHaveValidationErrorFor(paymentDemand => paymentDemand.PaymentMethod.Cvv);
        }

        [Fact]
        public void ShouldHaveErrorWhenCardNumberIsInvalid()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = this.ValidPaymentDemand;
            invalidPaymentDemand.PaymentMethod.Number = "Invalid Card Number";

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldHaveValidationErrorFor(paymentDemand => paymentDemand.PaymentMethod.Number);
        }

    }
}
