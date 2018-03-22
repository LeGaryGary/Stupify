namespace Stupify.Data.SQL.Models
{
    internal class Quote
    {
        public int QuoteId { get; set; }
        public int ServerUserId { get; set; }
        public string QuoteBody { get; set; }

        public virtual ServerUser ServerUser { set; get; }
    }
}