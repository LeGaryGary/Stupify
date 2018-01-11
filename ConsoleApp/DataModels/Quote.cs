using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp.DataModels
{
    public class Quote
    {
        public long QuoteId { get; set; }
        public long ServerUserId { get; set; }
        public string QuoteBody { get; set; }
        
        public virtual ServerUser ServerUser { set; get; }
    }
}
