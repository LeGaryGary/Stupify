using System;
using System.Linq;
using Discord.Net.Udp;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp.DataModels
{
    public class BotContext : DbContext
    {
        public BotContext() : base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

        public DbSet<Server> Servers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ServerUser> ServerUsers { get; set; }
        public DbSet<Quote> Quotes { get; set; }

        public ServerUser GetServerUser(long serverId, long userId)
        {
            try
            {
                return ServerUsers.FirstOrDefault(x => x.Server.ServerId == serverId && x.User.UserId == userId) ??
                       AddServerUser(serverId, userId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public ServerUser AddServerUser(long serverId, long userId)
        {
            var user = Users.FirstOrDefault(x => x.UserId == userId) ?? AddUser(userId);
            var server = Servers.FirstOrDefault(x => x.ServerId == serverId) ?? AddServer(serverId);
            ServerUsers.Add(new ServerUser()
            {
                User = user,
                Server = server
            });
            SaveChanges();
            return ServerUsers.FirstOrDefault(x => x.Server.ServerId == serverId && x.User.UserId == userId);
        }

        private Server AddServer(long serverId)
        {
            Servers.Add(new Server()
            {
                ServerId = serverId
            });
            SaveChanges();
            return Servers.FirstOrDefault(x => x.ServerId == serverId);
        }

        private User AddUser(long userId)
        {
            Users.Add(new User()
            {
                UserId = userId
            });
            SaveChanges();
            return Users.FirstOrDefault(x => x.UserId == userId);
        }
    }
}