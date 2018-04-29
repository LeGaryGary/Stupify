namespace Stupify.Data.SQL.Models
{
    internal class ServerTwitchChannel
    {
        public int ServerTwitchChannelId { get; set; }
        public Server Server { get; set; }
        public string TwitchLoginName { get; set; }
        public bool LastStatus { get; set; }
    }
}
