using Stupify.Data.Models;

namespace Stupify.Data.SQL.Models
{
    internal class ServerCustomText
    {
        public int ServerCustomTextId { get; set; }
        public Server Server { get; set; }
        public CustomText Type { get; set; }
        public string Text { get; set; }
    }
}
