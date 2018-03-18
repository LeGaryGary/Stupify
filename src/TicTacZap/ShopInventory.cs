using System;
using System.Collections.Generic;
using TicTacZap.Blocks;

namespace TicTacZap
{
    public class ShopInventory
    {
        public ShopInventory()
        {
            BlocksPrices.Add(BlockType.BasicEnergy, 100);
            BlocksPrices.Add(BlockType.BasicBeamer, 100);
            BlocksPrices.Add(BlockType.BasicWall, 20);
        }

        private Dictionary<BlockType, decimal> BlocksPrices { get; } = new Dictionary<BlockType, decimal>();

        public string TextRender()
        {
            var str = "Blocks:" + Environment.NewLine;

            foreach (var block in BlocksPrices) str += $"{block.Key}, Price: {block.Value} units" + Environment.NewLine;

            return str;
        }

        public decimal? GetBuyTotal(BlockType block, int quantity)
        {
            if (BlocksPrices.ContainsKey(block))
            {
                return BlocksPrices[block] * quantity * 1.05m;
            }

            return null;
        }

        public decimal? GetSellTotal(BlockType block, int quantity)
        {
            if (BlocksPrices.ContainsKey(block))
            {
                return BlocksPrices[block] * quantity * 0.95m;
            }

            return null;
        }
    }
}