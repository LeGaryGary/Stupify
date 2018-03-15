namespace TicTacZap.Blocks
{
    public interface IBlock
    {
        BlockType BlockType { get; }
        decimal Upkeep { get; }
        int MaxHealth { get; }
        int Health { get; set; }

        void DestroyThis();
    }
}