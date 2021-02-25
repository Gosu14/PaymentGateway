using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Common.Exceptions;

namespace PaymentGateway.Api.Filters
{
    /// <summary>
    /// Filter to manage application exceptions centrally.
    /// </summary>
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly IDictionary<Type, Action<ExceptionContext>> exceptionHandlers;

        private readonly ILogger<ApiExceptionFilter> logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            // Register known exception types and handlers.
            this.exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(ValidationException), this.HandleValidationException },
                { typeof(PaymentDeclineException), this.HandlePaymentDeclinedException },
                { typeof(AutoMapperMappingException), this.HandleMappingException },
                { typeof(NotFoundException), this.HandleNotFoundException },
            };
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnException(ExceptionContext context) => this.HandleException(context);

        private void HandleException(ExceptionContext context)
        {
            var type = context.Exception.GetType();
            if (this.exceptionHandlers.ContainsKey(type))
            {
                this.exceptionHandlers[type].Invoke(context);
                return;
            }

            this.HandleUnknownException(context);
        }

        private void HandleNotFoundException(ExceptionContext context)
        {
            var exception = context.Exception as NotFoundException;

            var details = new ProblemDetails()
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "The specified resource was not found.",
                Detail = exception?.Message
            };

            context.Result = new NotFoundObjectResult(details);

            context.ExceptionHandled = true;

            //this.logger.LogWarning("Not Found Exception : {Id} has been catch.", details.Detail);
            this.logger.LogWarning(exception, "A NotFoundException has been catch.");
        }

        private void HandlePaymentDeclinedException(ExceptionContext context)
        {
            var exception = context.Exception as PaymentDeclineException;

            context.Result = new ObjectResult(exception?.PaymentDeclined)
            {
                StatusCode = StatusCodes.Status402PaymentRequired
            };

            context.ExceptionHandled = true;

            this.logger.LogWarning(exception, "A PaymentDeclineException has been catch.");
        }

        private void HandleMappingException(ExceptionContext context)
        {
            var exception = context.Exception as AutoMapperMappingException;

            var details = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request formatting, please check the json payload format."
            };

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;

            this.logger.LogWarning(exception, "An AutoMapperMappingException has been catch.");
        }

        private void HandleValidationException(ExceptionContext context)
        {
            var exception = context.Exception as ValidationException;

            var details = new ValidationProblemDetails(exception?.Errors)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            };

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;

            this.logger.LogWarning(exception, "An ValidationException has been catch.");
        }

        private void HandleUnknownException(ExceptionContext context)
        {
            var exception = context.Exception;

            var details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;

            this.logger.LogCritical(exception, "An unmanaged exception has been catch.");
        }
    }
}
