namespace BotDataGraph.MessageAnalyser.Models
{
    /// <summary>
    ///     Server node for neo4J
    /// </summary>
    public class Server
    {
        private string serverId;

        public ulong ServerId
        {
            get => ulong.Parse(serverId);
            set => serverId = value.ToString();
        }
    }
}