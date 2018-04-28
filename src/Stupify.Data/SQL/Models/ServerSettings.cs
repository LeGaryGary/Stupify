namespace Stupify.Data.SQL.Models
{
    internal class ServerSettings
    {
        public int ServerSettingsId { get; set; }
        public Server Server { get; set; }
        public string CommandPrefix { get; set; }
        public string CustomCommandPrefix { get; set; }
        public long? ModeratorRoleId { get; set; }
        public long? WelcomeChannel { get; set; }
        public long? LeaveChannel { get; set; }
        public long? BanChannel { get; set; }
        public long? KickChannel { get; set; }
    }
}
