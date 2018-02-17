using System.Collections;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<ServerStory> ServerStories { get; set; }
        public DbSet<ServerStoryPart> ServerStoryParts { get; set; }
        public DbSet<Segment> Segments { get; set; }
    }
}