using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<PaymentConfirmation> PaymentConfirmations { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
