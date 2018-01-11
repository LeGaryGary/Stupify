using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp.DataModels
{
    public class Quote
    {
        public int QuoteId { get; set; }
        public int ServerUserId { get; set; }
        public string QuoteBody { get; set; }
        
        public virtual ServerUser ServerUser { set; get; }
    }
}
