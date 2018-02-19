namespace TicTacZap.Segment.Blocks
{
    public interface IBlock
    {
        BlockType BlockType { get; }
        decimal Upkeep { get; }
    }
}