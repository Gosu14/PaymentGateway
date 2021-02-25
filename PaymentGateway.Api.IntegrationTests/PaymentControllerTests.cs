using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaymentGateway.Api.Dto;
using Xunit;

namespace PaymentGateway.Api.IntegrationTests
{
    public class PaymentControllerTests : IDisposable
    {
        private readonly TestServer server;
        private readonly HttpClient client;
        private readonly IConfiguration configuration;

        public string ValidRequest { get; } =
            $"{{\r\n  \"amount\": 500,\r\n  \"currency\": \"usd\",\r\n  \"paymentMethod\": {{\r\n    \"paymentType\": \"card\",\r\n    \"cardBrand\": \"visa\",\r\n    \"cardCountry\": \"fr\",\r\n    \"cardExpiryMonth\": \"03\",\r\n    \"cardExpiryYear\": \"2030\",\r\n    \"cardNumber\": \"4977 9494 9494 9497\",\r\n    \"cardCvv\": \"737\"\r\n  }}\r\n}}";

        public string InvalidRequestWithStolenCard { get; } =
            "{\r\n  \"amount\": 500,\r\n  \"currency\": \"usd\",\r\n  \"paymentMethod\": {\r\n    \"paymentType\": \"card\",\r\n    \"cardBrand\": \"visa\",\r\n    \"cardCountry\": \"us\",\r\n    \"cardExpiryMonth\": \"03\",\r\n    \"cardExpiryYear\": \"2030\",\r\n    \"cardNumber\": \"4000 0200 0000 0000\",\r\n    \"cardCvv\": \"737\"\r\n  }\r\n}";

        public string InvalidRequestWithValidationException { get; } =
            "{\r\n  \"amount\": 500,\r\n  \"currency\": \"zzz\",\r\n  \"paymentMethod\": {\r\n    \"paymentType\": \"card\",\r\n    \"cardBrand\": \"visa\",\r\n    \"cardCountry\": \"us\",\r\n    \"cardExpiryMonth\": \"03\",\r\n    \"cardExpiryYear\": \"2030\",\r\n    \"cardNumber\": \"4000 0200 0000 0000\",\r\n    \"cardCvv\": \"737\"\r\n  }\r\n}";

        public string InvalidRequest { get; } =
            "{\r\n  \"amount\": 0,\r\n  \"currency\": \"string\",\r\n  \"paymentMethod\": {\r\n    \"cardBrand\": \"string\",\r\n    \"cardCountry\": \"string\",\r\n    \"cardExpiryMonth\": \"string\",\r\n    \"cardExpiryYear\": \"string\",\r\n    \"cardNumber\": \"string\",\r\n    \"cardCvv\": \"string\"\r\n  }\r\n}";

        public string InvalidRequestWithFormatException { get; } = "{}";

        public string PaymentDemandUri { get; } = $"/payments/payment-demand";

        public string PaymentDetailUri { get; } = $"/payments/payment-details?id=";

        public PaymentControllerTests()
        {
            // Arrange
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();

            this.configuration = builder.Build();

            this.server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>().UseConfiguration(this.configuration));
            this.client = this.server.CreateClient();

            this.client.DefaultRequestHeaders.Add("ApiKey", this.configuration.GetValue<string>("ApiKey"));
        }

        [Fact]
        public async Task ShouldReturnStatusCodeUnauthorizedIfApiKeyIsMissing()
        {
            //Act
            using var httpClient = this.server.CreateClient();
            var response = await httpClient.PostAsync(this.PaymentDemandUri, GetPayload(this.ValidRequest));

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnStatusCodeOkWithValidRequest()
        {
            //Act
            var response = await this.client.PostAsync(this.PaymentDemandUri, GetPayload(this.ValidRequest));

            var paymentConfirmation =
                JsonConvert.DeserializeObject<PaymentConfirmationDto>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(paymentConfirmation);
            Assert.IsType<PaymentConfirmationDto>(paymentConfirmation);
        }

        [Fact]
        public async Task ShouldHaveInsertedInDbThePaymentConfirmationAndShouldBeRequestableById()
        {
            //Arrange
            var responseFromPost = await this.client.PostAsync(this.PaymentDemandUri, GetPayload(this.ValidRequest));

            //Act
            var paymentConfirmation =
                JsonConvert.DeserializeObject<PaymentConfirmationDto>(await responseFromPost.Content.ReadAsStringAsync());

            var responseFromGet = await this.client.GetAsync(this.PaymentDetailUri + paymentConfirmation.Id);

            var paymentConfirmationFromGet =
                JsonConvert.DeserializeObject<PaymentConfirmationDto>(await responseFromPost.Content.ReadAsStringAsync());

            //Assert
            Assert.Equal(HttpStatusCode.OK, responseFromGet.StatusCode);
            Assert.Equal(paymentConfirmation.Id, paymentConfirmationFromGet.Id);
            Assert.Equal(paymentConfirmation.Amount, paymentConfirmationFromGet.Amount);
            Assert.Equal(paymentConfirmation.CardBrand, paymentConfirmationFromGet.CardBrand);
            Assert.Equal(paymentConfirmation.CardCountry, paymentConfirmationFromGet.CardCountry);
            Assert.Equal(paymentConfirmation.CardExpiryYear, paymentConfirmationFromGet.CardExpiryYear);
            Assert.Equal(paymentConfirmation.Currency, paymentConfirmationFromGet.Currency);
            Assert.Equal(paymentConfirmation.Status, paymentConfirmationFromGet.Status);
            Assert.Equal(paymentConfirmation.Last4, paymentConfirmationFromGet.Last4);
        }

        [Fact]
        public async Task ShouldReturnHttpCode404WhenPaymentDetailIsNotFound()
        {
            //Act
            var responseFromGet = await this.client.GetAsync(this.PaymentDetailUri + "Wrong id");

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, responseFromGet.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnHttpCode402WhenCardIsStolen()
        {
            //Act
            var response = await this.client.PostAsync(this.PaymentDemandUri, GetPayload(this.InvalidRequestWithStolenCard));

            var paymentConfirmation =
                JsonConvert.DeserializeObject<PaymentConfirmationDto>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.Equal(HttpStatusCode.PaymentRequired, response.StatusCode);
            Assert.NotNull(paymentConfirmation);
            Assert.IsType<PaymentConfirmationDto>(paymentConfirmation);
        }

        [Fact]
        public async Task ShouldReturnHttpCode400WhenFormatIsWrong()
        {
            //Act
            var response = await this.client.PostAsync(this.PaymentDemandUri, GetPayload(this.InvalidRequestWithFormatException));

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnHttpCode400WhenFormatIsInvalid()
        {
            //Act
            var response = await this.client.PostAsync(this.PaymentDemandUri, GetPayload(this.InvalidRequest));

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnHttpCode400WhenValidationFail()
        {
            //Act
            var response = await this.client.PostAsync(this.PaymentDemandUri, GetPayload(this.InvalidRequestWithValidationException));

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static StringContent GetPayload(string request) => new(request, Encoding.UTF8, "application/json");

        public void Dispose()
        {
            this.client.Dispose();
            this.server.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
