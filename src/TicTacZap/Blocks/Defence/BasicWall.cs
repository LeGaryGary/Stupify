using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacZap.Blocks.Defence
{
    public class BasicWall : IBlock
    {
        public BlockType BlockType { get; } = BlockType.BasicWall;
        public decimal Upkeep { get; } = 0;
        public int MaxHealth { get; } = 200;
        public int Health { get; set; }

        public BasicWall()
        {
            Health = MaxHealth;
        }

        public void DestroyThis()
        {
            throw new NotImplementedException();
        }
    }
}
