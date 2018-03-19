namespace TicTacZap.Blocks
{
    public interface IEnergyConsumer : IBlock
    {
        int EnergyConsumption { get; }
    }
}
