using System;
using System.Text;
using System.Threading.Tasks;

namespace TicTacZap
{
    public class Universe
    {
        public (int x, int y) Center;
        public int?[,] Segments;
        public int Size;

        public Universe(int size)
        {
            Size = size;
            Segments = new int?[size, size];
            Center = (Size / 2, Size / 2);
        }

        public string RenderRelative((int x, int y) location, int scope)
        {
            var str = new StringBuilder();
            for (var y = location.y+scope; y >= location.y-scope; y--)
            {
                for (var x = location.x - scope; x < location.x + scope + 1; x++)
                {
                    if (x < 0 || y < 0 ||
                        x >= Size ||
                        y >= Size)
                    {
                        str.Append("   ");
                        continue;
                    }

                    str.Append(Segments[x, y] != null ? " S " : " ~ ");
                }

                str.Append(Environment.NewLine);
            }

            return str.ToString();
        }

        public async Task<(int x, int y)?> NewSegmentAsync(int segmentId)
        {
            var locationNullable = await GetFirstEmptySegmentAsync().ConfigureAwait(false);
            if (!locationNullable.HasValue) return null;
            (var x, int y) = ((int, int)) locationNullable;
            Segments[x, y] = segmentId;
            return (x, y);
        }

        public void DeleteSegment((int x, int y) coords)
        {
            Segments[coords.x, coords.y] = null;
        }

        private Task<(int x, int y)?> GetFirstEmptySegmentAsync()
        {
            return FindSegmentAsync(null);
        }

        public Task<(int x, int y)?> FindSegmentAsync(int? segmentId)
        {
            (int x, int y)? FindSegment()
            {
                var x = Center.x;
                var y = Center.y;
                var layer = 0;
                var direction = Direction.Up;
                try
                {
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
                            else
                            {
                                y++;
                            }

                            break;

                        case Direction.Down:
                            if (Math.Abs(y - Center.y) == layer)
                            {
                                direction = Direction.Left;
                                x--;
                            }
                            else
                            {
                                y--;
                            }

                            break;

                        case Direction.Left:
                            if (Math.Abs(x - Center.x) == layer)
                            {
                                direction = Direction.Up;
                                y++;
                            }
                            else
                            {
                                x--;
                            }

                            break;

                        case Direction.Right:
                            if (Math.Abs(x - Center.x) == layer)
                            {
                                direction = Direction.Down;
                                y--;
                            }
                            else
                            {
                                x++;
                            }

                            break;
                    }
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

                return (x, y);
            }

            return Task.Run(() => FindSegment());
        }
    }
}