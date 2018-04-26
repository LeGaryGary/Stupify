namespace Stupify.Data.Models
{
    public class ServerSettings
    {
        public string GuildName { get; set; }

        public string CommandPrefix { get; set; }
        public string CustomCommandPrefix { get; set; }
        public ulong? ModeratorRoleId { get; set; }
    }
}
