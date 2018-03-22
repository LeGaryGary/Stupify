namespace Stupify.Data.SQL.Models
{
    public class Quote
    {
        public int QuoteId { get; set; }
        public int ServerUserId { get; set; }
        public string QuoteBody { get; set; }

        public virtual ServerUser ServerUser { set; get; }
    }
}