using System.Collections.Generic;

namespace Stupify.Data.SQL.Models
{
    public class Server
    {
        public Server()
        {
            StoryInProgress = false;
        }

        public int ServerId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DiscordGuildId { get; set; }

        public bool StoryInProgress { get; set; }

        public virtual ICollection<ServerUser> ServerUsers { get; set; }
        public virtual ICollection<ServerStory> ServerStories { get; set; }
    }
}