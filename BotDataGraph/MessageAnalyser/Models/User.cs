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
            get => ulong.Parse(this.userId);
            set => this.userId = value.ToString();
        }

        public object Parameters()
        {
            return new { this.userId };
        }
    }
}
