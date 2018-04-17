namespace Stupify.Data.SQL.Models
{
    internal class CustomCommand
    {
        public int CustomCommandId { get; set; }
        public ServerUser ServerUser { get; set; }
        public string CommandTag { get; set; }
        public string Command { get; set; }
    }
}
