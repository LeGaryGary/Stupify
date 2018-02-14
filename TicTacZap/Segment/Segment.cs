﻿using System;
using System.Text;
using TicTacZap.Segment.Blocks;

namespace TicTacZap.Segment
{
    public class Segment : ISegment
    {
        private int _lastY;
        private int _lastX;

        public IBlock Controller { get; } = new SegmentControllerBlock();

        public IBlock[,] Blocks { get; } = new IBlock[9,9];

        public decimal OutputPerTick { get; private set; }

        public Segment()
        {
            Blocks[4, 4] = Controller;
        }

        public bool AddBlock(int x, int y, BlockType blockType)
        {
            var block = NewBlock(blockType);
            return AddBlock(x, y, block);
        }

        public bool AddBlock(int x, int y, IBlock block)
        {
            if (Blocks[x, y] != null) return false;

            Blocks[x, y] = block;
            UpdateOutputs();
            return true;
        }

        private void UpdateOutputs()
        {
            var output = 0m;
            for (var y = 0; y < Blocks.GetLength(1); y++)
            {
                for (var x = 0; x < Blocks.GetLength(0); x++)
                {
                    var block = Blocks[x, y];
                    block.UpdateOutput();
                    output += block.OutputPerTick;
                }
            }

            OutputPerTick = output;
        }

        private static IBlock NewBlock(BlockType blockType)
        {
            IBlock block;

            switch (blockType)
            {
                case BlockType.Controller:
                    block = new SegmentControllerBlock();
                    break;
                case BlockType.Basic:
                    block = new BasicSegmentBlock();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(blockType), blockType, null);
            }

            return block;
        }

        private int BlockOutputDistance(int x, int y)
        {
            return Math.Abs(x - _lastX) + Math.Abs(y - _lastY);
        }

        private int DistanceToBlock(int x, int y, Direction direction)
        {
            GetBlockInDirection(x, y, direction);
            return BlockOutputDistance(x, y);
        }

        private IBlock GetBlockInDirection(int x, int y, Direction direction)
        {
            try
            {
                IBlock block;
                switch (direction)
                {                    
                    case Direction.Up:
                        block = Blocks[x,y+1];
                        if (block == null) return GetBlockInDirection(x, y + 1, direction);
                        _lastX = x;
                        _lastY = y + 1;
                        return block;

                    case Direction.Down:
                        block = Blocks[x,y-1];
                        if (block == null) return GetBlockInDirection(x, y - 1, direction);
                        _lastX = x;
                        _lastY = y - 1;
                        return block;

                    case Direction.Left:
                        block = Blocks[x-1,y];
                        if (block == null) return GetBlockInDirection(x - 1, y, direction);
                        _lastX = x - 1;
                        _lastY = y;
                        return block;

                    case Direction.Right:
                        block = Blocks[x+1,y];
                        if (block == null) return GetBlockInDirection(x + 1, y, direction);
                        _lastX = x + 1;
                        _lastY = y;
                        return block;
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                if (e is IndexOutOfRangeException)
                {
                    return null;
                }
                throw;
            }
        }

        public string TextRender()
        {
            var stringBuilder = new StringBuilder();
            for (var y = 0; y < Blocks.GetLength(0); y++)
            {
                for (var x = 0; x < Blocks.GetLength(1); x++)
                {
                    switch (Blocks[x,y]?.Type)
                    {
                        case BlockType.Controller:
                            stringBuilder.Append(" C");
                            break;
                        case BlockType.Basic:
                            stringBuilder.Append(" B");
                            break;
                        default:
                            stringBuilder.Append(" ~ ");
                            break;
                    }
                }

                stringBuilder.Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }
    }

    public interface ISegment
    {
        IBlock Controller { get; }
        IBlock[,] Blocks { get; }
        
        decimal OutputPerTick { get; }

        bool AddBlock(int x, int y, BlockType blockType);
    }
}
