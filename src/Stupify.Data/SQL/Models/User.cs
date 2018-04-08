using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stupify.Data.SQL.Models
{
    internal class User
    {
        public int UserId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DiscordUserId { get; set; }

        public decimal Balance { get; set; }

        public virtual ICollection<ServerUser> ServerUsers { get; set; }
    }
}