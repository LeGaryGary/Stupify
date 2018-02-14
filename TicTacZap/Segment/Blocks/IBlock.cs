using System.Collections.Generic;

namespace TicTacZap.Segment.Blocks
{
    public interface IBlock
    {
        BlockType Type { get; }
        decimal OutputPerTick { get; }

        decimal UpdateOutput();
    }
}
