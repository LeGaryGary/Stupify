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
        private readonly IDiscordClient _client;

        public UserRepository(BotContext botContext, IDiscordClient client)
        {
            _botContext = botContext;
            _client = client;
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

        public async Task<bool> IsGuildOwnerAsync(ulong userId, ulong guildId)
        {
            return (await GetServerUserAsync(userId, guildId).ConfigureAwait(false)).IsOwner ?? false;
        }

        private Task<ServerUser> GetServerUserAsync(IGuildUser discordGuildUser)
        {
            return GetServerUserAsync(discordGuildUser.Id, discordGuildUser.Guild.Id);
        }

        private async Task<ServerUser> GetServerUserAsync(ulong discordUserId, ulong discordGuildId)
        {
            var server = await GetServerAsync(discordGuildId).ConfigureAwait(false);
            var user = await GetUserAsync(discordUserId).ConfigureAwait(false);
            var serverUser = await _botContext.ServerUsers.FirstOrDefaultAsync(
                su => su.User.UserId == user.UserId && su.Server.ServerId == server.ServerId).ConfigureAwait(false);
            if (serverUser != null)
            {
                if (serverUser.IsOwner != null) return serverUser;

                serverUser.IsOwner = await IsAdministratorAsync(discordUserId, discordGuildId).ConfigureAwait(false);
                await _botContext.SaveChangesAsync().ConfigureAwait(false);

                return serverUser;
            }

            serverUser = new ServerUser
            {
                Server = server,
                User = user,
                Muted = false,
                IsOwner = await IsAdministratorAsync(discordUserId, discordGuildId).ConfigureAwait(false)
            };
            _botContext.ServerUsers.Add(serverUser);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return serverUser;
        }

        private async Task<bool> IsAdministratorAsync(ulong discordUserId, ulong discordGuildId)
        {
            return (await (await _client.GetGuildAsync(discordGuildId).ConfigureAwait(false)).GetUserAsync(discordUserId).ConfigureAwait(false)).GuildPermissions.Administrator;
        }

        private  Task<Server> GetServerAsync(IGuild discordGuild)
        {
            return GetServerAsync(discordGuild.Id);
        }

        private async Task<Server> GetServerAsync(ulong discordGuildId)
        {
            var guildId = (long) discordGuildId;
            var guild = await _botContext.Servers.FirstOrDefaultAsync(s => s.DiscordGuildId == guildId).ConfigureAwait(false);
            if (guild != null) return guild;

            guild = new Server
            {
                DiscordGuildId = guildId,
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
