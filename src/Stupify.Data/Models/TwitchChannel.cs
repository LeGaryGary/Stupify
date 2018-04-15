using System;
using System.Collections.Generic;
using System.Text;

namespace Stupify.Data.Models
{
    public class TwitchChannel
    {
        public long GuildId { get; set; }
        public long UpdateChannel { get; set; }
        public string TwitchLoginName { get; set; }
        public bool LastStatus { get; set; }
    }
}
