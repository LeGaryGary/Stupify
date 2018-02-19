using System;

namespace BotDataGraph.MessageAnalyser.Models
{
    public class Message
    {
        private string channelId;

        private string serverId;

        // Neo4J params
        private string time;

        // Neo4J relationships
        private string userId;

        public DateTime Time
        {
            get => DateTime.Parse(time);
            set => time = value.ToString("s");
        }

        public string Content { get; set; }

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
            return new {userId, Content, time, serverId};
        }

        public object Parameters(string startup)
        {
            return new {userId, Content, time, serverId, channelId, startupTime = startup};
        }

        public object Parameters(string startup, int lastNode)
        {
            return new {userId, Content, time, serverId, channelId, startupTime = startup, lastNodeId = lastNode};
        }
    }
}