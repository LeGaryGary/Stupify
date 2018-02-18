using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TicTacZap
{
    public class Universe
    {
        private int?[,] _segments;

        public Universe()
        {
            _segments = new int?[1000,1000];
        }

        public (int,int) NewSegment(int segmentId)
        {
            (var x, var y) = GetFirstEmptySegment();
            _segments[x, y] = segmentId;
            return (x,y);
        }

        public void DeleteSegment(int x, int y)
        {
            _segments[x, y] = null;
        }

        private (int, int) GetFirstEmptySegment()
        {
            return FindSegment(null);
        }

        public (int, int) FindSegment(int? segmentId)
        {
            var x = 500;
            var y = 500;
            var layer = 1;
            var direction = Direction.Up;
            while (true)
            {
                if (_segments[x, y] == segmentId) break;

                if (Math.Max(Math.Abs(x - 500), Math.Abs(y - 500)) == layer)
                {
                    switch (direction)
                    {
                        case Direction.Up:
                            direction = Direction.Right;
                            break;
                        case Direction.Down:
                            direction = Direction.Left;
                            break;
                        case Direction.Left:
                            direction = Direction.Up;
                            break;
                        case Direction.Right:
                            direction = Direction.Down;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                switch (direction)
                {
                    case Direction.Up:
                        y++;
                        break;
                    case Direction.Down:
                        y--;
                        break;
                    case Direction.Left:
                        x--;
                        break;
                    case Direction.Right:
                        x++;
                        break;
                }
            }

            return (x, y);
        }
    }
}
