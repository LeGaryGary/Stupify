using System.Collections.Generic;

namespace StupifyConsoleApp.DataModels
{
    public class ServerUser
    {
        public int ServerUserId { get; set; }
        public int UserId { get; set; }
        public int ServerId { get; set; }
        public bool Muted { get; set; }

        public virtual User User { get; set; }
        public virtual Server Server { get; set; }

        public virtual ICollection<Quote> Quotes { get; set; }
    }
}