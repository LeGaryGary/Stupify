using System;

namespace TicTacZap.Blocks
{
    public abstract class BaseBlock : IBlock
    {
        public int MaxHealth { get; protected set; }
        public int Health { get; set; }
        public string UnicodeHealth()
        {
            return this.GetUnicodeHealth();
        }

        public BlockType BlockType { get; protected set; }
        public decimal Upkeep { get; protected set; }

        public void DestroyThis()
        {
        }

        public BlockInfo RenderBlockInfo()
        {
            return new BlockInfo
            {
                Type = BlockType,
                Health = Health,
                MaxHealth = MaxHealth
            };
        }
    }
}
