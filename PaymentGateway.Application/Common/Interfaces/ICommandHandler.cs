
namespace PaymentGateway.Application.Common.Interfaces
{
    using System.Threading.Tasks;

    public interface ICommandHandler<in T, TU>
    {
        public Task<TU> ExecuteAsync(T command);
    }
}
