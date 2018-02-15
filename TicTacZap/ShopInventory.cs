using System;
using System.Collections.Generic;
using TicTacZap.Segment.Blocks;

namespace TicTacZap
{
    public class ShopInventory
    {
        private Dictionary<BlockType, decimal> BlocksPrices { get; set; } = new Dictionary<BlockType, decimal>();

        public ShopInventory()
        {
            BlocksPrices.Add(BlockType.Basic, 100);
        }

        public string TextRender()
        {
            var str = "Blocks:" + Environment.NewLine;

            foreach (var block in BlocksPrices)
            {
                str += $"{block.Key}, Price: {block.Value} units" + Environment.NewLine;
            }

            return str;
        }

        public decimal GetTotal(BlockType block, int quantity)
        {
            return BlocksPrices[block] * quantity;
        }
    }
}
