namespace TicTacZap.Blocks
{
    public interface IBlock : IHealth
    {
        BlockType BlockType { get; }
        decimal Upkeep { get; }

        void DestroyThis();
        BlockInfo RenderBlockInfo();
    }
}