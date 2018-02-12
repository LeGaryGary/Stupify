using System;
using System.Collections.Generic;
using System.Text;

namespace StupifyConsoleApp.DataModels
{
    public class Segment
    {
        public int SegmentId { get; set; }
        public int UserId { get; set; }
        public decimal OutputPerTick { get; set; }
    }
}
