namespace TicTacZap.Segment.Blocks
{
    public class SegmentControllerBlock : IBlock
    {
        public SegmentControllerBlock()
        {
            BlockType = BlockType.Controller;
        }

        public BlockType BlockType { get; }
        public decimal Upkeep { get; } = 0;
    }
}