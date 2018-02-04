using System;

namespace BotDataGraph.MessageAnalyser.Models
{
    public class Message
    {
        // Neo4J params
        private string time;
        private string content;
        
        // Neo4J relationships
        private string userId;
        private string serverId;
        private string channelId;

        public DateTime Time
        {
            get => DateTime.Parse(this.time);
            set => this.time = value.ToString("s");
        }

        public string Content
        {
            get => this.content; 
            set => this.content = value;
        }

        public ulong UserId
        {
            get => ulong.Parse(this.userId);
            set => this.userId = value.ToString();
        }

        public ulong ServerId
        {
            get => ulong.Parse(this.serverId);
            set => this.serverId = value.ToString();
        }

        public ulong ChannelId
        {
            get => ulong.Parse(this.channelId);
            set => this.channelId = value.ToString();
        }

        public object Parameters()
        {
            return new { this.userId, this.content, this.time, this.serverId };
        }

        public object Parameters(string startup)
        {
            return new { this.userId, this.content, this.time, this.serverId, this.channelId, startupTime = startup };
        }
    }
}