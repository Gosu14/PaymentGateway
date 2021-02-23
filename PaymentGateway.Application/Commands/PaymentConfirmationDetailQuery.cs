using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Commands
{
    public class PaymentConfirmationDetailQuery : ICommandHandler<string, PaymentConfirmation>
    {
        private readonly IApplicationDbContext dbContext;

        public PaymentConfirmationDetailQuery(IApplicationDbContext dbContext) => this.dbContext = dbContext;

        public async Task<PaymentConfirmation> ExecuteAsync(string command) => await this.dbContext.PaymentConfirmations.FirstAsync(pc => pc.Id.ToString() == command);
    }
}
