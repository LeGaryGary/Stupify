using System.Threading.Tasks;
using Discord;

namespace Stupify.Data.Repositories
{
    public interface IUserRepository
    {
        Task<bool> IsMutedAsync(IGuildUser user);
        Task MuteAsync(IGuildUser user);
        Task<bool> UnMuteAsync(IGuildUser user);
        Task<decimal> BalanceAsync(IUser user);
        Task<bool> BalanceTransferAsync(IUser from, IUser to, decimal amount);
        Task<bool> BankToUserTransferAsync(IUser user, decimal amount);
        Task<bool> UserToBankTransferAsync(IUser user, decimal amount);
        Task<int> GetUserIdAsync(IUser user);
    }
}
