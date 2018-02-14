using System.Collections.Generic;

namespace TicTacZap.Segment.Blocks
{
    internal abstract class BaseSegmentBlock : IBlock
    {
        public BlockType Type { get; protected set; }
        public decimal OutputPerTick { get; protected set; } = 1;
        public decimal UpdateOutput()
        {
            OutputPerTick = 1;
            //foreach (var inputBlock in InputBlocks)
            //{
            //    OutputPerTick += inputBlock.OutputPerTick * inputBlock.OutputDistance;
            //}

            return OutputPerTick;
        }
    }
}
