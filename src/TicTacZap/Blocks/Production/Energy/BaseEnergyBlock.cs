namespace TicTacZap.Blocks.Production.Energy
{
    public abstract class BaseEnergyBlock : BaseBlock, IProduceBlock
    {
        public Resource OutputType { get; } = Resource.Energy;
        public decimal OutputPerTick { get; private set; }

        public decimal UpdateOutput(int sumOfDistancesInDirections, int neighbours)
        {
            OutputPerTick = sumOfDistancesInDirections / (decimal) (neighbours + 1);
            return OutputPerTick;
        }
    }
}