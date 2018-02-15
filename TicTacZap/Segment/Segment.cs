using System;
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

        public decimal OutputPerTick { get; set; }

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

        public bool DeleteBlock(int x, int y)
        {
            if (Blocks[x, y] is SegmentControllerBlock) return false;
            Blocks[x, y] = null;
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
                    if (Blocks[x,y] == null) continue;

                    var block = Blocks[x, y];

                    block.UpdateOutput(
                        DistanceSumInDirections(x, y),
                        ConnectedDiagonals(x, y),
                        Layer(x, y));
                    output += block.OutputPerTick;
                }
            }

            OutputPerTick = output;
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

        private int DistanceToBlock(int x, int y, Direction direction)
        {
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

        public static IBlock NewBlock(BlockType blockType)
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

        public string TextRender()
        {
            var stringBuilder = new StringBuilder();
            for (var y = Blocks.GetLength(0)-1; y >= 0; y--)
            {
                for (var x = 0; x < Blocks.GetLength(1); x++)
                {
                    switch (Blocks[x,y]?.Type)
                    {
                        case BlockType.Controller:
                            stringBuilder.Append(" C ");
                            break;
                        case BlockType.Basic:
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

    public interface ISegment
    {
        IBlock Controller { get; }
        IBlock[,] Blocks { get; }
        
        decimal OutputPerTick { get; }

        bool AddBlock(int x, int y, BlockType blockType);
    }
}
