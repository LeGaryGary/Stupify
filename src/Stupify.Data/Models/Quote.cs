using Discord;

namespace Stupify.Data.Models
{
    public class Quote
    {
        public string Content { get; set; }
        public IUser Author { get; set; }
    }
}
