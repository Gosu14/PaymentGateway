using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace PaymentGateway.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException()
            : base("Validation exceptions have happened.") => this.Errors = new Dictionary<string, string[]>();

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this() => this.Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}
