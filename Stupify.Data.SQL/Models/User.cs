using System.Collections.Generic;

namespace Stupify.Data.SQL.Models
{
    public class User
    {
        public int UserId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DiscordUserId { get; set; }

        public decimal Balance { get; set; }

        public virtual ICollection<ServerUser> ServerUsers { get; set; }
    }
}