using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StupifyConsoleApp.DataModels
{
    public class Server
    {
        public int ServerId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DiscordGuildId { get; set; }

        public virtual ICollection<ServerUser> ServerUsers { get; set; }

        public Server()
        {
        }

        public Server(long discordGuildId)
        {
            DiscordGuildId = discordGuildId;
        }
    }
}