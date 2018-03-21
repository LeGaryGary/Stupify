namespace TicTacZap.Blocks.Production.Energy
{
    public class BasicEnergyBlock : BaseEnergyBlock
    {
        public BasicEnergyBlock()
        {
            BlockType = BlockType.Energy;
            Upkeep = 0;
            MaxHealth = 10;
            Health = MaxHealth;
        }
    }
}