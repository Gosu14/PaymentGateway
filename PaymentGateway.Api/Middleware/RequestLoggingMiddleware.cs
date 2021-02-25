using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Api.Middleware
{
    /// <summary>
    /// Logging middleware to log requests input & outputs
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        private readonly Stopwatch timer;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.timer = new Stopwatch();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                this.logger.LogInformation(
                    "Request {method} {url} ApiKey: {ApiKey} - received at {utc}",
                    context.Request?.Method,
                    context.Request?.Path.Value,
                    context.Request.Headers?["ApiKey"],
                    DateTime.UtcNow);

                this.timer.Start();

                await this.next(context);

                this.timer.Stop();
            }
            finally
            {
                this.logger.LogInformation(
                    "Request {method} {url} ApiKey: {ApiKey} => {statusCode} - Running for {timer} milliseconds - returned at {utc}",
                    context.Request?.Method,
                    context.Request?.Path.Value,
                    context.Request.Headers?["ApiKey"],
                    context.Response?.StatusCode,
                    this.timer.ElapsedMilliseconds,
                    DateTime.UtcNow);
            }
        }
    }
}
