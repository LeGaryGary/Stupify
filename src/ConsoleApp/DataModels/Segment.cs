namespace StupifyConsoleApp.DataModels
{
    public class Segment
    {
        public int SegmentId { get; set; }

        public decimal UnitsPerTick { get; set; }
        public decimal EnergyPerTick { get; set; }

        public decimal Energy { get; set; }

        public virtual User User { get; set; }
    }
}