using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Stupify.Data
{
    public interface IUserRepository
    {
        Task<bool> IsMutedAsync(IGuildUser user);
        Task MuteAsync(IGuildUser user);
        Task<bool> UnMuteAsync(IGuildUser user);
        Task<decimal> BalanceAsync(IUser user);
    }
}
