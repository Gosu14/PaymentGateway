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
                Country = "gb",
                Cvv = 500,
                Number = "5555 5555 5555 4444",
                ExpiryYear = 2025,
                ExpiryMonth = 11
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
            var invalidPaymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "usd",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = "gb",
                    Cvv = 500,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2019,
                    ExpiryMonth = 11
                }
            };

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
            var invalidPaymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "usd",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = "gb",
                    Cvv = 500,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2020,
                    ExpiryMonth = 01
                }
            };

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
            var invalidPaymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "us",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = "gb",
                    Cvv = 500,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldHaveValidationErrorFor(paymentDemand => paymentDemand.Currency);
        }

        [Fact]
        public void ShouldHaveErrorWhenCurrencyIsNotManaged()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "zzz",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = "gb",
                    Cvv = 500,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

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
            var paymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = currency,
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = "gb",
                    Cvv = 500,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

            //Act
            var result = validator.TestValidate(paymentDemand);

            //Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void ShouldHaveErrorWhenCountryIssuingCardIsNotManaged()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "usd",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = "zz",
                    Cvv = 500,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldHaveValidationErrorFor(paymentDemand => paymentDemand.PaymentMethod.Country);
        }

        [Fact]
        public void ShouldHaveErrorWhenCountryIssuingCardIsInvalid()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "usd",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = "country",
                    Cvv = 500,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

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
            var paymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "usd",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = country,
                    Cvv = 500,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

            //Act
            var result = validator.TestValidate(paymentDemand);

            //Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void ShouldHaveErrorWhenCardBrandIsNotManaged()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "usd",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "American Express",
                    Country = "gb",
                    Cvv = 500,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldHaveValidationErrorFor(paymentDemand => paymentDemand.PaymentMethod.Brand);
        }

        public static IEnumerable<object[]> GetValidCardBrand() => PaymentDataConstants.CardBrandManaged.Select(brand => new object[] { brand });

        [Theory]
        [MemberData(nameof(GetValidCardBrand))]
        public void ShouldNotHaveErrorWhenCardBrandIsManaged(string brand)
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var paymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "usd",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = brand,
                    Country = "gb",
                    Cvv = 500,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

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
            var invalidPaymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "usd",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = "gb",
                    Cvv = 55555,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

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
            var invalidPaymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "usd",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = "gb",
                    Cvv = 555,
                    Number = "Invalid Card Number",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldHaveValidationErrorFor(paymentDemand => paymentDemand.PaymentMethod.Number);
        }

        [Fact]
        public void ShouldNotHaveErrorWhenCardNumberIsValid()
        {
            //Arrange
            this.DateServiceMock.Setup(x => x.CurrentDateTime).Returns(new DateTime(2020, 01, 01));
            var validator = new PaymentRequestValidator(this.DateServiceMock.Object);
            var invalidPaymentDemand = new PaymentDemand
            {
                Amount = 500,
                Currency = "usd",
                PaymentMethod = new PaymentMethod()
                {
                    Brand = "visa",
                    Country = "gb",
                    Cvv = 555,
                    Number = "5555 5555 5555 4444",
                    ExpiryYear = 2025,
                    ExpiryMonth = 11
                }
            };

            //Act
            var result = validator.TestValidate(invalidPaymentDemand);

            //Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
