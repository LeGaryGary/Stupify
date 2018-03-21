namespace TicTacZap.Blocks.Defence
{
    public class BasicWall : BaseBlock
    {

        public BasicWall()
        {
            BlockType = BlockType.Wall;
            MaxHealth = 200;
            Health = MaxHealth;
            Upkeep = 0;
        }
    }
}
