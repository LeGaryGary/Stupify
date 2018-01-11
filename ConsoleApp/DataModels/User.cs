using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConsoleApp.DataModels
{
    public class User
    {
        public int UserId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DiscordUserId { get; set; }

        public virtual ICollection<ServerUser> ServerUsers { get; set; }
    }
}
