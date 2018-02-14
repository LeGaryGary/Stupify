using System.Collections.Generic;
using TicTacZap.Block;

namespace TicTacZap.Segment.Blocks
{
    internal abstract class BaseSegmentBlock : IBlock
    {
        public BlockType Type { get; protected set; }
        public List<IBlock> InputBlocks { get; } = new List<IBlock>();
        public IBlock OutputBlock { get; protected set; }
        public decimal OutputPerTick { get; protected set; } = 1;
        public int OutputDistance { get; set; } = 1;

        public bool AddInput(IBlock block)
        {
            InputBlocks.Add(block);
            if (!block.LoopsExists())
            {
                InputBlocks.Remove(block);
                return false;
            }

            UpdateOutput();
            return true;
        }

        public bool SetOutputBlock(IBlock block)
        {
            OutputBlock = block;
            return true;
        }

        public void RemoveInput(IBlock block)
        {
            InputBlocks.Remove(block);
            UpdateOutput();
        }

        public decimal UpdateOutput()
        {
            OutputPerTick = 1;
            foreach (var inputBlock in InputBlocks)
            {
                OutputPerTick += inputBlock.OutputPerTick * inputBlock.OutputDistance;
            }

            return OutputPerTick;
        }
    }
}
