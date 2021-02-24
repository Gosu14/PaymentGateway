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
    [ApiController]
    [Route("payments")]
    [ApiExceptionFilter]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> logger;
        private readonly IMapper paymentDtoMapper;

        public PaymentController(ILogger<PaymentController> logger, IMapper paymentDtoMapper)
        {
            this.logger = logger;
            this.paymentDtoMapper = paymentDtoMapper;
        }

        [HttpPost("payment-demand")]
        public async Task<IActionResult> PaymentRequest([FromServices] ICommandHandler<PaymentDemand, PaymentConfirmation> command, [FromBody] PaymentDemandDto req)
        {
            this.logger.LogInformation($"Payment demandDto received at : {DateTime.UtcNow}");
            var paymentDemand = this.paymentDtoMapper.Map<PaymentDemand>(req);
            var paymentConfirmation = await command.ExecuteAsync(paymentDemand);
            var paymentConfirmationDto = this.paymentDtoMapper.Map<PaymentConfirmationDto>(paymentConfirmation);
            return new OkObjectResult(paymentConfirmationDto);
        }

        [HttpGet("payment-details")]
        public async Task<IActionResult> PaymentRequest([FromServices] ICommandHandler<string, PaymentConfirmation> command, [FromQuery] string id)
        {
            this.logger.LogInformation($"Payment demandDto received at : {DateTime.UtcNow}");
            var paymentConfirmation = await command.ExecuteAsync(id);
            var paymentConfirmationDto = this.paymentDtoMapper.Map<PaymentConfirmationDto>(paymentConfirmation);
            return new OkObjectResult(paymentConfirmationDto);
        }
    }
}
