using System;

namespace PaymentGateway.Application.Common.Interfaces
{
    public interface IDateService
    {
        DateTime CurrentDateTime { get; }
    }
}
