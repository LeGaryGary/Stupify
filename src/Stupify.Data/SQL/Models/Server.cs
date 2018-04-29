using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stupify.Data.SQL.Models
{
    internal class Server
    {
        public Server()
        {
            StoryInProgress = false;
        }

        public int ServerId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DiscordGuildId { get; set; }

        public bool StoryInProgress { get; set; }
        public long? TwitchUpdateChannel { get; set; }

        public virtual ICollection<ServerUser> ServerUsers { get; set; }
        public virtual ICollection<ServerStory> ServerStories { get; set; }
    }
}