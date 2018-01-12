namespace StupifyConsoleApp.DataModels
{
    public class Quote
    {
        public int QuoteId { get; set; }
        public int ServerUserId { get; set; }
        public string QuoteBody { get; set; }
        
        public virtual ServerUser ServerUser { set; get; }
    }
}
