using System.Collections.Generic;
using TicTacZap.Segment.Blocks;

namespace TicTacZap.Block
{
    public interface IBlock
    {
        BlockType Type { get; }
        List<IBlock> InputBlocks { get; }
        IBlock OutputBlock { get; }
        decimal OutputPerTick { get; }
        int OutputDistance { get; set; }

        bool AddInput(IBlock block);
        void RemoveInput(IBlock block);
        bool SetOutputBlock(IBlock block);

        decimal UpdateOutput();
    }
}
