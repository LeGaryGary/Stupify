
namespace TicTacZap.Segment.Blocks
{
    public class SegmentControllerBlock : IBlock
    {
        public BlockType BlockType { get; }
        public decimal Upkeep { get; } = 0;

        public SegmentControllerBlock()
        {
            BlockType = BlockType.Controller;
        }
    }
}
