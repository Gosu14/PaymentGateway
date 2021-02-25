using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Api.Filters
{
    /// <summary>
    /// Filter to manage authentication through an API Key in RequestHeader.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiAuthenticationAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "ApiKey";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiAuthenticationAttribute>>();

            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKey))
            {
                context.Result = new UnauthorizedResult();
                logger?.LogWarning("ApiKey missing in request Header. Returning HTTP CODE 401 Not Authorized.");
                return;
            }

            //Get mock API Key from configurations
            var conf = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var mockApiKey = conf.GetValue<string>("ApiKey");

            if (!(mockApiKey == apiKey))
            {
                context.Result = new UnauthorizedResult();
                logger?.LogWarning("Invalid ApiKey: {ApiKey} in request Header. Returning HTTP CODE 401 Not Authorized.", apiKey);
                return;
            }

            await next();
        }
    }
}
