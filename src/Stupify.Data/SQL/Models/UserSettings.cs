using System;
using System.Collections.Generic;
using System.Text;

namespace Stupify.Data.SQL.Models
{
    internal class UserSettings
    {
        public int UserSettingsId { get; set; }
        public User User { get; set; }
    }
}
