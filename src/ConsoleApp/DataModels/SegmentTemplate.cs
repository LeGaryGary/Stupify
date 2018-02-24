using System;
using System.Collections.Generic;
using System.Text;

namespace StupifyConsoleApp.DataModels
{
    public class SegmentTemplate
    {
        public int SegmentTemplateId { get; set; }
        public virtual User User { get; set; }
        public string Name { get; set; }
    }
}
