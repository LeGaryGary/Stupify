using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TicTacZap
{
    public class Universe
    {
        public readonly int?[,] Segments;
        public readonly int Size;
        public readonly (int x, int y) Center;

        public Universe(int size)
        {
            Size = size;
            Segments = new int?[size,size];
            Center = (Size / 2, Size / 2);
        }

        public string RenderTheEntiretyOfCreationAsWeKnowIt()
        {
            var str = string.Empty;
            for (var y = Size-1; y >= 0; y--)
            {
                for (var x = 0; x < Size; x++)
                {
                    if (Segments[x, y] != null) str += " S ";
                    else str += " ~ ";
                }

                str += Environment.NewLine;
            }

            return str;
        }

        public (int,int) NewSegment(int segmentId)
        {
            (var x, var y) = GetFirstEmptySegment();
            Segments[x, y] = segmentId;
            return (x,y);
        }

        public void DeleteSegment((int x, int y) coords)
        {
            Segments[coords.x, coords.y] = null;
        }

        private (int, int) GetFirstEmptySegment()
        {
            return FindSegment(null);
        }

        public (int, int) FindSegment(int? segmentId)
        {
            var x = Center.x;
            var y = Center.y;
            var layer = 0;
            var direction = Direction.Up;
            while (true)
            {
                if (Segments[x, y] == segmentId) break;

                switch (direction)
                {
                    case Direction.Up:
                        if (Math.Abs(y - Center.y) == layer)
                        {
                            y++;
                            layer++;
                            direction = Direction.Right;
                        }
                        else y++;
                        break;
                    case Direction.Down:
                        if (Math.Abs(y - Center.y) == layer)
                        {
                            direction = Direction.Left;
                            x--;
                        }
                        else y--;
                        break;
                    case Direction.Left:
                        if (Math.Abs(x - Center.x) == layer)
                        {
                            direction = Direction.Up;
                            y++;
                        }
                        else x--;
                        break;
                    case Direction.Right:
                        if (Math.Abs(x - Center.x) == layer)
                        {
                            direction = Direction.Down;
                            y--;
                        }
                        else x++;
                        break;
                }
            }

            return (x, y);
        }
    }
}
