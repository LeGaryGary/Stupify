using System;
using System.Collections.Generic;
using System.Text;
using TicTacZap.Blocks;
using TicTacZap.Blocks.Production;

namespace TicTacZap
{
    public class Segment
    {
        private int _lastX;

        private int _lastY;

        public Segment()
        {
            Blocks[4, 4] = Controller;

            ResourceOutput = new Dictionary<Resource, decimal>();
            foreach (Resource resource in Enum.GetValues(typeof(Resource))) ResourceOutput.Add(resource, 0);
        }

        public IBlock Controller { get; } = new SegmentControllerBlock();
        public IBlock[,] Blocks { get; } = new IBlock[9, 9];

        internal Dictionary<Resource, decimal> ResourceOutput { get; set; }

        public bool AddBlock(int x, int y, BlockType blockType)
        {
            var block = TicTacZapExtensions.NewBlock(blockType, x, y);
            return AddBlock(x, y, block);
        }

        public bool AddBlock(int x, int y, IBlock block)
        {
            if (Blocks[x, y] != null) return false;

            Blocks[x, y] = block;
            UpdateSegmentOutput();
            return true;
        }

        public BlockType? DeleteBlock(int x, int y)
        {
            if (Blocks[x, y] is SegmentControllerBlock) return null;
            var blockType = Blocks[x, y]?.BlockType;
            Blocks[x, y] = null;
            if (blockType != null) UpdateSegmentOutput();
            return blockType;
        }

        public void UpdateSegmentOutput()
        {
            ResourceOutput = new Dictionary<Resource, decimal>();

            for (var y = 0; y < Blocks.GetLength(1); y++)
            for (var x = 0; x < Blocks.GetLength(0); x++)
            {
                if (Blocks[x, y] == null) continue;

                var block = Blocks[x, y];
                UpdateResourceOutput(Resource.Unit, -block.Upkeep);

                if (!(block is IProduceBlock produceBlock)) continue;

                produceBlock.UpdateOutput(
                    DistanceSumInDirections(x, y),
                    Neighbours(x, y));
                UpdateResourceOutput(produceBlock.OutputType, produceBlock.OutputPerTick);
            }
        }

        private int Neighbours(int x, int y)
        {
            var total = 0;
            for (var i = -1; i <= 1; i++)
            for (var j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                try
                {
                    if (Blocks[x + i, y + j] != null) total++;
                }
                catch (Exception e)
                {
                    if (!(e is IndexOutOfRangeException)) throw;
                }
            }

            return total;
        }

        private int DistanceSumInDirections(int x, int y)
        {
            var sum = 0;
            sum += DistanceToBlock(x, y, Direction.Up);
            sum += DistanceToBlock(x, y, Direction.Down);
            sum += DistanceToBlock(x, y, Direction.Left);
            sum += DistanceToBlock(x, y, Direction.Right);
            return sum;
        }

        private int DistanceToBlock(int x, int y, Direction direction)
        {
            _lastX = x;
            _lastY = y;
            GetBlockInDirection(x, y, direction);
            return BlockOutputDistance(x, y);
        }

        private int BlockOutputDistance(int x, int y)
        {
            return Math.Abs(x - _lastX) + Math.Abs(y - _lastY);
        }

        private IBlock GetBlockInDirection(int x, int y, Direction direction)
        {
            try
            {
                IBlock block;
                switch (direction)
                {
                    case Direction.Up:
                        block = Blocks[x, y + 1];
                        if (block == null) return GetBlockInDirection(x, y + 1, direction);
                        _lastX = x;
                        _lastY = y + 1;
                        return block;

                    case Direction.Down:
                        block = Blocks[x, y - 1];
                        if (block == null) return GetBlockInDirection(x, y - 1, direction);
                        _lastX = x;
                        _lastY = y - 1;
                        return block;

                    case Direction.Left:
                        block = Blocks[x - 1, y];
                        if (block == null) return GetBlockInDirection(x - 1, y, direction);
                        _lastX = x - 1;
                        _lastY = y;
                        return block;

                    case Direction.Right:
                        block = Blocks[x + 1, y];
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
                if (e is IndexOutOfRangeException) return null;
                throw;
            }
        }

        private void UpdateResourceOutput(Resource produceBlockOutputType, decimal produceBlockOutputPerTick)
        {
            if (ResourceOutput.ContainsKey(produceBlockOutputType))
            {
                ResourceOutput[produceBlockOutputType] += produceBlockOutputPerTick;
                return;
            }

            ResourceOutput.Add(produceBlockOutputType, produceBlockOutputPerTick);
        }

        public string TextRender()
        {
            var stringBuilder = new StringBuilder();
            for (var y = Blocks.GetLength(0) - 1; y >= 0; y--)
            {
                for (var x = 0; x < Blocks.GetLength(1); x++)
                    switch (Blocks[x, y]?.BlockType)
                    {
                        case BlockType.Controller:
                            stringBuilder.Append(" C ");
                            break;
                        case BlockType.Energy:
                            stringBuilder.Append(" B ");
                            break;
                        case BlockType.Wall:
                            stringBuilder.Append(" W ");
                            break;
                        case BlockType.Beamer:
                            stringBuilder.Append(" Q ");
                            break;
                        default:
                            stringBuilder.Append(" ~ ");
                            break;
                    }

                stringBuilder.Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }

        public string HealthTextRender()
        {
            var stringBuilder = new StringBuilder();
            for (var y = Blocks.GetLength(0) - 1; y >= 0; y--)
            {
                for (var x = 0; x < Blocks.GetLength(1); x++)
                {
                    var block = Blocks[x, y];
                    if (block == null)
                    {
                        stringBuilder.Append("➖");
                        continue;
                    }

                    stringBuilder.Append(block.UnicodeHealth());
                }

                stringBuilder.Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }
    }
}