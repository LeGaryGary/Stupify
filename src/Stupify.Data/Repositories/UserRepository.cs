using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.SQL;
using Stupify.Data.SQL.Models;

namespace Stupify.Data.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly BotContext _botContext;

        public UserRepository(BotContext botContext)
        {
            _botContext = botContext;
        }

        public async Task<bool> IsMutedAsync(IGuildUser user)
        {
            var serverUser = (await GetServerUserAsync(user).ConfigureAwait(false));
            return serverUser.Muted;
        }

        public async Task MuteAsync(IGuildUser user)
        {
            var su = await GetServerUserAsync(user).ConfigureAwait(false);
            su.Muted = true;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<bool> UnMuteAsync(IGuildUser user)
        {
            var su = await GetServerUserAsync(user).ConfigureAwait(false);
            if (!su.Muted) return false;

            su.Muted = false;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        public async Task<decimal> BalanceAsync(IUser user)
        {
            return (await GetUserAsync(user).ConfigureAwait(false)).Balance;
        }

        public async Task<bool> BalanceTransferAsync(IUser from, IUser to, decimal amount)
        {
            var fromUser = await GetUserAsync(@from).ConfigureAwait(false);
            var toUser = await GetUserAsync(to).ConfigureAwait(false);

            if (fromUser.Balance - amount < 0) return false;

            fromUser.Balance -= amount;
            toUser.Balance += amount;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        public async Task<bool> BankToUserTransferAsync(IUser user, decimal amount)
        {
            var fromUser = await _botContext.GetBankAsync().ConfigureAwait(false);
            var toUser = await GetUserAsync(user).ConfigureAwait(false);

            if (fromUser.Balance - amount < 0) return false;

            fromUser.Balance -= amount;
            toUser.Balance += amount;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        public async Task<bool> UserToBankTransferAsync(IUser user, decimal amount)
        {
            var fromUser = await GetUserAsync(user).ConfigureAwait(false);
            var toUser = await _botContext.GetBankAsync().ConfigureAwait(false);

            if (fromUser.Balance - amount < 0) return false;

            fromUser.Balance -= amount;
            toUser.Balance += amount;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        public async Task<int> GetUserIdAsync(IUser user)
        {
            return (await GetUserAsync(user).ConfigureAwait(false)).UserId;
        }

        public async Task<List<ulong>> UsersGuildsAsync(ulong discordGuildId)
        {
            var user = await GetUserAsync(discordGuildId).ConfigureAwait(false);
            var userId = user.UserId;
            var serverUsers = await _botContext.ServerUsers
                .Where(su => su.User.UserId == userId)
                .Include(su => su.Server)
                .ToArrayAsync().ConfigureAwait(false);

            return serverUsers.Select(su => (ulong) su.Server.DiscordGuildId).ToList();
        }

        private async Task<ServerUser> GetServerUserAsync(IGuildUser discordGuildUser)
        {
            var server = await GetServerAsync(discordGuildUser.Guild).ConfigureAwait(false);
            var user = await GetUserAsync(discordGuildUser).ConfigureAwait(false);
            var serverUser = await _botContext.ServerUsers.FirstOrDefaultAsync(
                su => su.User.UserId == user.UserId && su.Server.ServerId == server.ServerId).ConfigureAwait(false);
            if (serverUser != null) return serverUser;

            serverUser = new ServerUser
            {
                Server = server,
                User = user,
                Muted = false
            };
            _botContext.ServerUsers.Add(serverUser);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return serverUser;
        }

        private async Task<Server> GetServerAsync(IGuild discordGuild)
        {
            var guild = await _botContext.Servers.FirstOrDefaultAsync(s => s.DiscordGuildId == (long)discordGuild.Id).ConfigureAwait(false);
            if (guild != null) return guild;

            guild = new Server
            {
                DiscordGuildId = (long)discordGuild.Id,
                StoryInProgress = false
            };
            _botContext.Servers.Add(guild);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return guild;
        }

        private Task<User> GetUserAsync(IUser discordUser)
        {
            return GetUserAsync(discordUser.Id);
        }

        private Task<User> GetUserAsync(ulong discordUserId)
        {
            return GetUserAsync((long) discordUserId);
        }

        private async Task<User> GetUserAsync(long discordUserId)
        {
            

            var user = await _botContext.Users.FirstOrDefaultAsync(u => u.DiscordUserId == discordUserId).ConfigureAwait(false);
            if (user != null) return user;

            user = new User
            {
                Balance = 1000,
                DiscordUserId = discordUserId
            };
            _botContext.Users.Add(user);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return user;
        }
    }
}
