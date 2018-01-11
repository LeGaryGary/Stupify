using System.Collections;
using System.Collections.Generic;

namespace ConsoleApp.DataModels
{
    public class ServerUser
    {
        public long ServerUserId { get; set; }
        public long UserId { get; set; }
        public long ServerId { get; set; }

        public virtual User User { get; set; }
        public virtual Server Server { get; set; }

        public virtual ICollection<Quote> Quotes { get; set; }
    }
}