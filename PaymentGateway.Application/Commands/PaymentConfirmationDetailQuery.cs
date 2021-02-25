using System;
using System.Threading.Tasks;
using PaymentGateway.Application.Common.Exceptions;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Commands
{
    public class PaymentConfirmationDetailQuery : ICommandHandler<string, PaymentConfirmation>
    {
        private readonly IApplicationDbContext dbContext;

        public PaymentConfirmationDetailQuery(IApplicationDbContext dbContext) => this.dbContext = dbContext;

        public async Task<PaymentConfirmation> ExecuteAsync(string command)
        {
            var guid = ConvertIdToGuidOrThrowAnException(command);
            return await this.TryGetEntityAsync(guid);
        }

        private static Guid ConvertIdToGuidOrThrowAnException(string id)
        {
            if (!Guid.TryParse(id, out var guid))
            {
                throw new NotFoundException(nameof(PaymentConfirmation), id);
            }

            return guid;
        }

        private async Task<PaymentConfirmation> TryGetEntityAsync(Guid guid)
        {
            var entity = await this.dbContext.PaymentConfirmations.FindAsync(guid);

            if (entity == null)
            {
                throw new NotFoundException(nameof(PaymentConfirmation), guid.ToString());
            }

            return entity;
        }
    }
}
