using System;
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
            return (await GetServerUserAsync(user)).Muted;
        }

        public async Task MuteAsync(IGuildUser user)
        {
            var su = await GetServerUserAsync(user);
            su.Muted = true;
            await _botContext.SaveChangesAsync();
        }

        public async Task<bool> UnMuteAsync(IGuildUser user)
        {
            var su = await GetServerUserAsync(user);
            if (!su.Muted) return false;

            su.Muted = false;
            await _botContext.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> BalanceAsync(IUser user)
        {
            return (await GetUserAsync(user)).Balance;
        }

        public async Task<bool> BalanceTransferAsync(IUser from, IUser to, decimal amount)
        {
            var fromUser = await GetUserAsync(from);
            var toUser = await GetUserAsync(to);

            if (fromUser.Balance - amount < 0) return false;

            fromUser.Balance -= amount;
            toUser.Balance += amount;
            await _botContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> BankToUserTransferAsync(IUser user, decimal amount)
        {
            var fromUser = await _botContext.GetBankAsync();
            var toUser = await GetUserAsync(user);

            if (fromUser.Balance - amount < 0) return false;

            fromUser.Balance -= amount;
            toUser.Balance += amount;
            await _botContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UserToBankTransferAsync(IUser user, decimal amount)
        {
            var fromUser = await GetUserAsync(user);
            var toUser = await _botContext.GetBankAsync();

            if (fromUser.Balance - amount < 0) return false;

            fromUser.Balance -= amount;
            toUser.Balance += amount;
            await _botContext.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetUserId(IUser user)
        {
            return (await GetUserAsync(user)).UserId;
        }

        private async Task<ServerUser> GetServerUserAsync(IGuildUser discordGuildUser)
        {
            var server = await GetServerAsync(discordGuildUser.Guild);
            var user = await GetUserAsync(discordGuildUser);
            var serverUser = await _botContext.ServerUsers.FirstOrDefaultAsync(
                su => su.User.UserId == user.UserId && su.Server.ServerId == server.ServerId);
            if (serverUser != null) return serverUser;

            serverUser = new ServerUser
            {
                Server = server,
                User = user,
                Muted = false
            };
            _botContext.ServerUsers.Add(serverUser);
            await _botContext.SaveChangesAsync();

            return serverUser;
        }

        private async Task<Server> GetServerAsync(IGuild discordGuild)
        {
            var guild = await _botContext.Servers.FirstOrDefaultAsync(s => s.DiscordGuildId == (long)discordGuild.Id);
            if (guild != null) return guild;

            guild = new Server
            {
                DiscordGuildId = (long)discordGuild.Id,
                StoryInProgress = false
            };
            _botContext.Servers.Add(guild);
            await _botContext.SaveChangesAsync();

            return guild;
        }

        private async Task<User> GetUserAsync(IUser discordUser)
        {
            var user = await _botContext.Users.FirstOrDefaultAsync(u => u.DiscordUserId == (long)discordUser.Id);
            if (user != null) return user;

            user = new User
            {
                Balance = 1000,
                DiscordUserId = (long)discordUser.Id
            };
            _botContext.Users.Add(user);
            await _botContext.SaveChangesAsync();

            return user;
        }
    }
}
