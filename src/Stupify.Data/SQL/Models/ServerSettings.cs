using System;
using System.Collections.Generic;
using System.Text;

namespace Stupify.Data.SQL.Models
{
    internal class ServerSettings
    {
        public int ServerSettingsId { get; set; }
        public Server Server { get; set; }
        public string CommandPrefix { get; set; }
        public string CustomCommandPrefix { get; set; }
    }
}
