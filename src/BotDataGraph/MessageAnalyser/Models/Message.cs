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
            get => DateTime.Parse(time);
            set => time = value.ToString("s");
        }

        public string Content
        {
            get => content; 
            set => content = value;
        }

        public ulong UserId
        {
            get => ulong.Parse(userId);
            set => userId = value.ToString();
        }

        public ulong ServerId
        {
            get => ulong.Parse(serverId);
            set => serverId = value.ToString();
        }

        public ulong ChannelId
        {
            get => ulong.Parse(channelId);
            set => channelId = value.ToString();
        }

        public object Parameters()
        {
            return new { userId, content, time, serverId };
        }

        public object Parameters(string startup)
        {
            return new { userId, content, time, serverId, channelId, startupTime = startup };
        }

        public object Parameters(string startup, int lastNode)
        {
            return new { userId, content, time, serverId, channelId, startupTime = startup, lastNodeId = lastNode };
        }
    }
}