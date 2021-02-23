using System;

namespace PaymentGateway.Application.Services
{
    using Common.Interfaces;

    public class DateService : IDateService
    {
        public DateTime CurrentDateTime => DateTime.Now;
    }
}
