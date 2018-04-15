using System;
using System.Collections.Generic;
using System.Text;

namespace Stupify.Data.SQL.Models
{
    internal class CustomCommand
    {
        public int CustomCommandId { get; set; }
        public Server Server { get; set; }
        public string CommandTag { get; set; }
        public string Command { get; set; }
    }
}
