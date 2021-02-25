using System;
using PaymentGateway.Application.Common.Interfaces;

namespace PaymentGateway.Infrastructure.Services
{
    public class DateService : IDateService
    {
        public DateTime CurrentDateTime => DateTime.Now;
    }
}
