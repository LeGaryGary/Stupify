namespace Stupify.Data.SQL.Models
{
    internal class SegmentTemplate
    {
        public int SegmentTemplateId { get; set; }
        public virtual User User { get; set; }
        public string Name { get; set; }
    }
}
