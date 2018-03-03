namespace TicTacZap.Blocks.Production.Energy
{
    public class BasicEnergyBlock : BaseEnergyBlock
    {
        public BasicEnergyBlock()
        {
            BlockType = BlockType.BasicEnergy;
            Upkeep = 0;
        }
    }
}