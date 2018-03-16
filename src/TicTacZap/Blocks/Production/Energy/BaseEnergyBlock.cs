namespace TicTacZap.Blocks.Production.Energy
{
    public abstract class BaseEnergyBlock : IProduceBlock
    {
        public BlockType BlockType { get; protected set; }
        public Resource OutputType { get; protected set; } = Resource.Energy;
        public decimal OutputPerTick { get; private set; }
        public decimal Upkeep { get; protected set; }
        public int MaxHealth { get; protected set; }
        public int Health { get; set; }

        public decimal UpdateOutput(int sumOfDistancesInDirections, int neighbours)
        {
            OutputPerTick = sumOfDistancesInDirections / (decimal) (neighbours + 1);
            return OutputPerTick;
        }

        public void DestroyThis()
        {
        }
    }
}