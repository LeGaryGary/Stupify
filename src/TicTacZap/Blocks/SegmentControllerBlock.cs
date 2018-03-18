namespace TicTacZap.Blocks
{
    public class SegmentControllerBlock : BaseBlock
    {

        public SegmentControllerBlock()
        {
            BlockType = BlockType.Controller;
            MaxHealth = 50;
            Health = MaxHealth;
            Upkeep = 0;
        }
    }
}