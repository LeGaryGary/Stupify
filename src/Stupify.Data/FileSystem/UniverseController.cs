using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TicTacZap;

namespace Stupify.Data.FileSystem
{
    internal class UniverseController
    {
        private const string UniverseExtension = ".UNI";
        private readonly string _universePath;
        private string _universeDirectory;

        private Universe _universe;

        public UniverseController(string dataDirectory, string universeName)
        {
            _universeDirectory = $"{dataDirectory}/Universes";
            _universePath = $"{dataDirectory}/Universes/{universeName}{UniverseExtension}";
        }

        public async Task Start()
        {
            Directory.CreateDirectory(_universeDirectory);
            if (File.Exists(_universePath))
            {
                _universe = await LoadUniverseFile();
            }
            else
            {
                _universe = new Universe(999);
                await SaveUniverseFileAsync();
            }
        }

        public async Task<(int x, int y)?> FindAsync(int segmentId)
        {
            return await _universe.FindSegmentAsync(segmentId);
        }

        public async Task<string> RenderRelativeToSegmentAsync(int segmentId, int scope)
        {
            if (scope > 12) return null;
            var locationNullable = await _universe.FindSegmentAsync(segmentId);
            return locationNullable.HasValue ? _universe.RenderRelative(locationNullable.Value, scope) : null;
        }

        public async Task<(int, int)?> NewSegmentAsync(int segmentId)
        {
            var newSegmentCoords = await _universe.NewSegmentAsync(segmentId);
            await SaveUniverseFileAsync();
            return newSegmentCoords;
        }

        public async Task<(int, int)?> DeleteSegment(int segmentId)
        {
            var coords = await _universe.FindSegmentAsync(segmentId);
            if (!coords.HasValue) return null;
            _universe.DeleteSegment(((int x,int y))coords);
            await SaveUniverseFileAsync();
            return coords;
        }

        public async Task<int?> GetAdjacentSegmentInDirectionAsync(int originalSegment, Direction direction)
        {
            var locationNullable = await _universe.FindSegmentAsync(originalSegment);
            if (!locationNullable.HasValue) return null;
            var location = ((int x, int y)) locationNullable;

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

            if (location.x >= _universe.Segments.GetLength(0) ||
                location.x < 0 ||
                location.y >= _universe.Segments.GetLength(1) ||
                location.y < 0)
            {
                return null;
            }

            return _universe.Segments[location.x, location.y];
        }

        private async Task<Universe> LoadUniverseFile()
        {
            using (var stream = File.OpenText(_universePath))
            {
                var fileText = await stream.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Universe>(fileText);
            }
        }

        private async Task SaveUniverseFileAsync()
        {
            var fileText = JsonConvert.SerializeObject(_universe);
            using (var stream = File.CreateText(_universePath))
            {
                await stream.WriteAsync(fileText);
            };
        }
    }
}