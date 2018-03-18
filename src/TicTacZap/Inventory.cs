using System;
using System.Collections.Generic;
using TicTacZap.Blocks;

namespace TicTacZap
{
    public class Inventory
    {
        public Inventory(int startingBlocks)
        {
            AddBlocks(BlockType.BasicEnergy, startingBlocks);
        }

        public Dictionary<BlockType, int> Blocks { get; set; } = new Dictionary<BlockType, int>();

        public void Reset()
        {
            Blocks = new Dictionary<BlockType, int>();
        }

        public string TextRender()
        {
            if (Blocks.Count == 0) return "";

            var str = string.Empty;

            foreach (var block in Blocks) if(block.Value != 0) str += $"`{block.Key} block x{block.Value}`" + Environment.NewLine;

            return str;
        }

        public void AddBlocks(BlockType blockType, int quantity)
        {
            if (!Blocks.ContainsKey(blockType))
            {
                Blocks.Add(blockType, quantity);
                return;
            }

            Blocks[blockType] = Blocks[blockType] + quantity;
        }

        public bool RemoveBlocks(BlockType blockType, int quantity)
        {
            if (!Blocks.ContainsKey(blockType) || Blocks[blockType] < quantity) return false;
            Blocks[blockType] -= quantity;
            return true;
        }
    }
}