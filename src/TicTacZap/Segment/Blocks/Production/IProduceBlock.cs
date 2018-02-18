namespace TicTacZap.Segment.Blocks.Production
{
    public interface IProduceBlock : IBlock
    {
        Resource OutputType { get; }
        decimal OutputPerTick { get; }

        decimal UpdateOutput(int sumOfDistancesInDirections, int connectedDiagonals, int layer);
    }
}