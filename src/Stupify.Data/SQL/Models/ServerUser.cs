using System.Collections.Generic;

namespace Stupify.Data.SQL.Models
{
    internal class ServerUser
    {
        public int ServerUserId { get; set; }
        public int UserId { get; set; }
        public int ServerId { get; set; }

        public bool Muted { get; set; }
        public bool? IsOwner { get; set; }

        public virtual User User { get; set; }
        public virtual Server Server { get; set; }

        public virtual ICollection<Quote> Quotes { get; set; }
    }
}