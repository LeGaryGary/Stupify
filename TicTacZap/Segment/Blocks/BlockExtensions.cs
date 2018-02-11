using System.Collections.Generic;
using TicTacZap.Block;

namespace TicTacZap.Segment.Blocks
{
    public static class BlockExtensions
    {
        public static bool LoopsExists(this IBlock block)
        {
            return block.CheckOutputForLoop() && block.CheckInputsForLoops();
        }

        public static bool CheckInputsForLoops(this IBlock block, List<IBlock> seenBlocks = null)
        {
            if (seenBlocks == null)
            {
                seenBlocks = new List<IBlock>();
            }

            if (seenBlocks.Contains(block))
            {
                return false;
            }

            seenBlocks.Add(block);

            foreach (var inputBlock in block.InputBlocks)
            {
                inputBlock.CheckInputsForLoops(seenBlocks);
            }

            return true;
        }

        public static bool CheckOutputForLoop(this IBlock block)
        {
            var seenBlocks = new List<IBlock>();
            while (true)
            {
                if (seenBlocks.Contains(block))
                {
                    return false;
                }
                seenBlocks.Add(block);

                if (block is SegmentControllerBlock || block.OutputBlock == null)
                {
                    return true;
                }

                block = block.OutputBlock;
            }
        }
    }
}
