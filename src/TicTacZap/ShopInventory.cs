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
            BlocksPrices.Add(BlockType.BasicBeamer, 1000);
            BlocksPrices.Add(BlockType.BasicWall, 20);
        }

        private Dictionary<BlockType, decimal> BlocksPrices { get; } = new Dictionary<BlockType, decimal>();

        public string TextRender()
        {
            var str = "Blocks:" + Environment.NewLine;

            foreach (var block in BlocksPrices) str += $"{block.Key}, Price: {block.Value} units" + Environment.NewLine;

            return str;
        }

        public decimal GetTotal(BlockType block, int quantity)
        {
            return BlocksPrices[block] * quantity;
        }
    }
}