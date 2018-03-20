using Microsoft.EntityFrameworkCore;

namespace StupifyConsoleApp.DataModels
{
    public class BotContext : DbContext
    {
        public BotContext(DbContextOptions<BotContext> options) : base(options)
        {
        }

        public DbSet<Server> Servers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ServerUser> ServerUsers { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<ServerStory> ServerStories { get; set; }
        public DbSet<ServerStoryPart> ServerStoryParts { get; set; }
        public DbSet<Segment> Segments { get; set; }
        public DbSet<SegmentTemplate> SegmentTemplates { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(Config.DbConnectionString);
        //}
    }
}