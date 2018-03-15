namespace TicTacZap.Blocks
{
    public class SegmentControllerBlock : IBlock
    {
        public BlockType BlockType { get; }
        public decimal Upkeep { get; } = 0;
        public int MaxHealth { get; } = 50;
        public int Health { get; set; }

        public SegmentControllerBlock()
        {
            BlockType = BlockType.Controller;
            Health = MaxHealth;
        }

        public void DestroyThis()
        {
            throw new System.NotImplementedException();
        }
    }
}