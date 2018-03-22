using System;
using System.IO;
using System.Threading.Tasks;
using TicTacZap;

namespace Stupify.Data.FileSystem
{
    public static class UniverseController
    {
        private const string UniverseExtension = ".UNI";
        private static readonly string UniversePath;

        private static readonly Universe Universe;

        static UniverseController()
        {
            UniversePath = Config.DataDirectory + @"\Universes";
            Directory.CreateDirectory(UniversePath);
            if (File.Exists(UniversePath + @"\" + Config.UniverseName + UniverseExtension))
            {
                Universe = LoadUniverseFile();
            }
            else
            {
                Universe = new Universe(999);
                SaveUniverseFileAsync().GetAwaiter().GetResult();
            }
        }

        public static string RenderRelativeToSegment(int segmentId, int scope)
        {
            if (scope > 12) return null;
            var locationNullable = Universe.FindSegment(segmentId);
            if (!locationNullable.HasValue) return null;
            return Universe.RenderRelative(locationNullable.Value, scope);
        }

        public static async Task<(int, int)?> NewSegment(int segmentId)
        {
            var newSegmentCoords = Universe.NewSegment(segmentId);
            await SaveUniverseFileAsync();
            return newSegmentCoords;
        }

        public static async Task<(int, int)?> DeleteSegment(int segmentId)
        {
            var coords = Universe.FindSegment(segmentId);
            if (!coords.HasValue) return null;
            Universe.DeleteSegment(((int x,int y))coords);
            await SaveUniverseFileAsync();
            return coords;
        }

        public static async Task<int?> GetAdjacentSegmentInDirection(int originalSegment, Direction direction)
        {
            var locationNullable = Universe.FindSegment(originalSegment);
            if (!locationNullable.HasValue) return null;
            var location = ((int x, int y))locationNullable;

            switch (direction)
            {
                case Direction.Up:
                    location.y++;
                    break;
                case Direction.Down:
                    location.y--;
                    break;
                case Direction.Left:
                    location.x--;
                    break;
                case Direction.Right:
                    location.x++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (location.x >= Universe.Segments.GetLength(0) ||
                location.x < 0 ||
                location.y >= Universe.Segments.GetLength(1) ||
                location.y < 0)
            {
                return null;
            }

            return Universe.Segments[location.x, location.y];
        }

        private static Universe LoadUniverseFile()
        {
            var fileText = File.ReadAllText(UniversePath + @"\" + Config.UniverseName + UniverseExtension);
            return JsonConvert.DeserializeObject<Universe>(fileText);
        }

        private static async Task SaveUniverseFileAsync()
        {
            var fileText = JsonConvert.SerializeObject(Universe);
            await File.WriteAllTextAsync(UniversePath + @"\" + Config.UniverseName + UniverseExtension, fileText);
        }
    }
}