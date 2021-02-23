using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Commands;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Interfaces;

namespace PaymentGateway.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddTransient<IDateService, DateService>();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddTransient<IAcquiringBankGateway, AcquiringBankGateway>();
            services.AddTransient<ICommandHandler<PaymentDemand, PaymentConfirmation>, PaymentCommandHandler>();
            services.AddTransient<ICommandHandler<string, PaymentConfirmation>, PaymentConfirmationDetailQuery>();
            return services;
        }
    }
}
