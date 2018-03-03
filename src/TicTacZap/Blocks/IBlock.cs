namespace TicTacZap.Blocks
{
    public interface IBlock
    {
        BlockType BlockType { get; }
        decimal Upkeep { get; }
    }
}