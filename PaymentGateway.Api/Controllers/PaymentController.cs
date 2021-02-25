using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Api.Dto;
using AutoMapper;
using PaymentGateway.Api.Filters;

namespace PaymentGateway.Api.Controllers
{
    /// <summary>
    /// PaymentController. Main Controller of the Application
    /// </summary>
    [ApiController]
    [Route("payments")]
    [ApiAuthentication]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> logger;
        private readonly IMapper paymentDtoMapper;

        public PaymentController(ILogger<PaymentController> logger, IMapper paymentDtoMapper)
        {
            this.logger = logger;
            this.paymentDtoMapper = paymentDtoMapper;
        }

        /// <summary>
        /// PaymentRequest method called via HttpPost on route : payment-demand
        /// Manage a payment request
        /// </summary>
        /// <param name="command">Payment request command handler. Injected from Services.</param>
        /// <param name="req">Payment demand data request. Injected from request Body.</param>
        /// <returns></returns>
        [HttpPost("payment-demand")]
        public async Task<IActionResult> PaymentRequest([FromServices] ICommandHandler<PaymentDemand, PaymentConfirmation> command, [FromBody] PaymentDemandDto req)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (req == null)
            {
                return new BadRequestObjectResult("Input request is null - Please check the JSON Payload");
            }

            return await this.ProcessPaymentRequest(command, req);
        }

        /// <summary>
        /// GetPaymentConfirmation method called via HttpGet on route: payment-details
        /// Return PaymentConfirmation data from ID.
        /// </summary>
        /// <param name="command">Payment confirmation query handler. Injected from Services.</param>
        /// <param name="id">PaymentConfirmation Id request Parameter. Injected from Query.</param>
        /// <returns></returns>
        [HttpGet("payment-details")]
        public async Task<IActionResult> GetPaymentConfirmation([FromServices] ICommandHandler<string, PaymentConfirmation> command, [FromQuery] string id)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return new BadRequestObjectResult("PaymentConfirmation Id parameters is empty - Please check your request");
            }

            return await this.ExecutePaymentConfirmationQuery(command, id);
        }

        private async Task<IActionResult> ExecutePaymentConfirmationQuery(ICommandHandler<string, PaymentConfirmation> command, string id)
        {
            this.logger.LogInformation($"Payment demandDto received at : {DateTime.UtcNow}");
            var paymentConfirmation = await command.ExecuteAsync(id);
            var paymentConfirmationDto = this.paymentDtoMapper.Map<PaymentConfirmationDto>(paymentConfirmation);
            return new OkObjectResult(paymentConfirmationDto);
        }

        private async Task<IActionResult> ProcessPaymentRequest(ICommandHandler<PaymentDemand, PaymentConfirmation> command, PaymentDemandDto req)
        {
            this.logger.LogInformation($"Payment demandDto received at : {DateTime.UtcNow}");
            var paymentDemand = this.paymentDtoMapper.Map<PaymentDemand>(req);
            var paymentConfirmation = await command.ExecuteAsync(paymentDemand);
            var paymentConfirmationDto = this.paymentDtoMapper.Map<PaymentConfirmationDto>(paymentConfirmation);
            return new OkObjectResult(paymentConfirmationDto);
        }
    }
}
