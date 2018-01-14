using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp.DataModels
{
    public class BotContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Config.DbConnectionString);
        }

        public DbSet<Server> Servers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ServerUser> ServerUsers { get; set; }
        public DbSet<Quote> Quotes { get; set; }

        public async Task<ServerUser> GetServerUserAsync(long userId, long serverId)
        {
            var serverUser = await ServerUsers.FirstOrDefaultAsync(
                x => x.Server.DiscordGuildId == serverId && x.User.DiscordUserId == userId);

            if (serverUser != null) return serverUser;

            //No ServerUser Exists => create one

            var userTask = Users.FirstOrDefaultAsync(
                x => x.DiscordUserId == userId);
            var serverTask = Servers.FirstOrDefaultAsync(
                x => x.DiscordGuildId == serverId);

            await ServerUsers.AddAsync(new ServerUser()
            {
                User = await userTask ?? new User(userId),
                Server = await serverTask ?? new Server(serverId)
            });
            await SaveChangesAsync();

            return await ServerUsers.FirstOrDefaultAsync(
                       su => su.Server.DiscordGuildId == serverId &&
                             su.User.DiscordUserId == userId) 
                   ?? throw new InvalidOperationException("Newly added serveruser not found!");
        }

        public string UsernameFromServerUser(ServerUser quoteServerUser)
        {
            var discordUserId = quoteServerUser.User.DiscordUserId;
            var guild = ClientManager.Client.GetGuild(((ulong) quoteServerUser.Server.DiscordGuildId));
            var user = guild.GetUser((ulong) discordUserId);
            return user.Username;
        }
    }
}