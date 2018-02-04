
namespace BotDataGraph.MessageAnalyser.Models
{
    /// <summary>
    /// Server node for neo4J
    /// </summary>
    public class Server
    {
        private string serverId;

        public ulong ServerId
        {
            get => ulong.Parse(this.serverId);
            set => this.serverId = value.ToString();
        }
    }
}
