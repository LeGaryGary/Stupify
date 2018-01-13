using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
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

        public ServerUser GetServerUser(long userId, long serverId)
        {
            var serverUser = ServerUsers.FirstOrDefault(
                x => x.Server.DiscordGuildId == serverId && x.User.DiscordUserId == userId);

            if (serverUser != null) return serverUser;

            var user = Users.FirstOrDefault(
                x => x.DiscordUserId == userId) ?? new User(userId);
            var server = Servers.FirstOrDefault(
                x => x.DiscordGuildId == serverId) ?? new Server(serverId);

            ServerUsers.Add(new ServerUser()
            {
                User = user,
                Server = server
            });
            SaveChanges();

            return ServerUsers.FirstOrDefault(
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