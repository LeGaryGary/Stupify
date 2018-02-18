using System;
using System.Collections.Generic;
using System.Text;
using TicTacZap.Segment.Blocks;
using TicTacZap.Segment.Blocks.Production;
using TicTacZap.Segment.Blocks.Production.Energy;

namespace TicTacZap.Segment
{
    public class Segment
    {
        public IBlock Controller { get; } = new SegmentControllerBlock();
        public IBlock[,] Blocks { get; } = new IBlock[9,9];

        internal Dictionary<Resource, decimal> ResourceOutput { get; set; }

        public Segment()
        {
            Blocks[4, 4] = Controller;

            ResourceOutput = new Dictionary<Resource, decimal>();
            foreach (Resource resource in Enum.GetValues(typeof(Resource)))
            {
                ResourceOutput.Add(resource, 0);
            }
        }

        public bool AddBlock(int x, int y, BlockType blockType)
        {
            var block = TicTacZapExtensions.NewBlock(blockType);
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

        private void UpdateSegmentOutput()
        {
            ResourceOutput = new Dictionary<Resource, decimal>();

            for (var y = 0; y < Blocks.GetLength(1); y++)
            {
                for (var x = 0; x < Blocks.GetLength(0); x++)
                {
                    if (Blocks[x,y] == null) continue;

                    var block = Blocks[x, y];

                    if (block is IProduceBlock produceBlock)
                    {
                        produceBlock.UpdateOutput(
                            DistanceSumInDirections(x, y),
                            ConnectedDiagonals(x, y),
                            Layer(x, y));

                        UpdateResourceOutput(produceBlock.OutputType, produceBlock.OutputPerTick);
                    }

                }
            }
        }

        private int Layer(int x, int y)
        {
            return Math.Max(Math.Abs(x - 4), Math.Abs(y - 4));
        }

        private int ConnectedDiagonals(int x, int y)
        {
            var diagonals = 0;
            try
            {
                diagonals = Blocks[x - 1, y - 1] == null ? diagonals:diagonals+1;
            }
            catch (Exception e)
            {
                if (!(e is IndexOutOfRangeException)) throw;
            }
            try
            {
                diagonals = Blocks[x + 1, y - 1] == null ? diagonals:diagonals+1;
            }
            catch (Exception e)
            {
                if (!(e is IndexOutOfRangeException)) throw;
            }
            try
            {
                diagonals = Blocks[x + 1, y + 1] == null ? diagonals:diagonals+1;
            }
            catch (Exception e)
            {
                if (!(e is IndexOutOfRangeException)) throw;
            }
            try
            {
                diagonals = Blocks[x - 1, y + 1] == null ? diagonals:diagonals+1;
            }
            catch (Exception e)
            {
                if (!(e is IndexOutOfRangeException)) throw;
            }

            return diagonals;
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

        private int _lastY;

        private int _lastX;

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
            for (var y = Blocks.GetLength(0)-1; y >= 0; y--)
            {
                for (var x = 0; x < Blocks.GetLength(1); x++)
                {
                    switch (Blocks[x,y]?.BlockType)
                    {
                        case BlockType.Controller:
                            stringBuilder.Append(" C ");
                            break;
                        case BlockType.BasicEnergy:
                            stringBuilder.Append(" B ");
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
}
