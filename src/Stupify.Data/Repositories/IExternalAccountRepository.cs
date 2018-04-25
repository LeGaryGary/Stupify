using System.Threading.Tasks;
using Stupify.Data.Models;

namespace Stupify.Data.Repositories
{
    public interface IExternalAccountRepository
    {
        Task<ExternalAuthentication> GetAsync(ulong discordUserId, ExternalService service);
        Task AddAsync(ulong discordUserId, ExternalService service,ExternalAuthentication authentication);
    }
}
