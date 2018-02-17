using System;
using System.Collections.Generic;
using System.Text;

namespace BotDataGraph.MessageAnalyser.Models
{
    using System.Runtime.CompilerServices;

    public class User
    {
        private string userId;

        public ulong UserId
        {
            get => ulong.Parse(userId);
            set => userId = value.ToString();
        }

        public object Parameters()
        {
            return new { userId };
        }
    }
}
