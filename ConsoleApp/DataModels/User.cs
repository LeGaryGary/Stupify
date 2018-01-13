using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StupifyConsoleApp.DataModels
{
    public class User
    {
        public int UserId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DiscordUserId { get; set; }

        public virtual ICollection<ServerUser> ServerUsers { get; set; }

        public User()
        {
        }

        public User(long discordUserId)
        {
            DiscordUserId = discordUserId;
        }
    }
}
