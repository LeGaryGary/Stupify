using System;
using System.Collections.Generic;

namespace TicTacZap.Segment.Blocks
{
    internal abstract class BaseSegmentBlock : IBlock
    {
        public BlockType Type { get; protected set; }
        public decimal OutputPerTick { get; protected set; } = 1;
        public decimal UpdateOutput(int sumOfDistancesInDirections, int connectedDiagonals, int layer)
        {
            var output = layer * layer * Math.Log(sumOfDistancesInDirections,2) * connectedDiagonals;
            OutputPerTick = Convert.ToDecimal(output);
            return Convert.ToDecimal(output);
        }
    }
}
