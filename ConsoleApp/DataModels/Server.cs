using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp.DataModels
{
    public class Server
    {
        public int ServerId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DiscordGuildId { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}