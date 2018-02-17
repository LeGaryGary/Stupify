using System;

namespace TicTacZap.Segment.Blocks.Production.Energy
{
    internal abstract class BaseEnergyBlock : IProduceBlock
    {
        public BlockType BlockType { get; protected set; }
        public Resource OutputType { get; protected set; } = Resource.Energy;
        public decimal OutputPerTick { get; private set; }
        public decimal Upkeep { get; protected set; }

        public decimal UpdateOutput(int sumOfDistancesInDirections, int connectedDiagonals, int layer)
        {
            var output = layer * layer * Math.Log(sumOfDistancesInDirections,2) * connectedDiagonals;
            if (double.IsNaN(output) || double.IsInfinity(output))
            {
                OutputPerTick = 0;
                return 0;
            }
            OutputPerTick = Convert.ToDecimal(output);
            return Convert.ToDecimal(output);
        }
    }
}
