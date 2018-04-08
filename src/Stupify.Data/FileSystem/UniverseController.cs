using System;
using System.Collections.Generic;
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

        public async Task StartAsync()
        {
            Directory.CreateDirectory(_universeDirectory);
            if (File.Exists(_universePath))
            {
                _universe = await LoadUniverseFileAsync().ConfigureAwait(false);
            }
            else
            {
                _universe = new Universe(999);
                await SaveUniverseFileAsync().ConfigureAwait(false);
            }
        }

        public IEnumerable<int> UniverseSegments()
        {
            foreach (var segment in _universe.Segments)
            {
                if (segment.HasValue) yield return segment.Value;
            }
        }

        public Task<(int x, int y)?> FindAsync(int segmentId)
        {
            return _universe.FindSegmentAsync(segmentId);
        }

        public async Task<string> RenderRelativeToSegmentAsync(int segmentId, int scope)
        {
            if (scope > 12) return null;
            var locationNullable = await _universe.FindSegmentAsync(segmentId).ConfigureAwait(false);
            return locationNullable.HasValue ? _universe.RenderRelative(locationNullable.Value, scope) : null;
        }

        public async Task<(int, int)?> NewSegmentAsync(int segmentId)
        {
            var newSegmentCoords = await _universe.NewSegmentAsync(segmentId).ConfigureAwait(false);
            await SaveUniverseFileAsync().ConfigureAwait(false);
            return newSegmentCoords;
        }

        public async Task<(int, int)?> DeleteSegmentAsync(int segmentId)
        {
            var coords = await _universe.FindSegmentAsync(segmentId).ConfigureAwait(false);
            if (!coords.HasValue) return null;
            _universe.DeleteSegment(((int x,int y))coords);
            await SaveUniverseFileAsync().ConfigureAwait(false);
            return coords;
        }

        public async Task<int?> GetAdjacentSegmentInDirectionAsync(int originalSegment, Direction direction)
        {
            var locationNullable = await _universe.FindSegmentAsync(originalSegment).ConfigureAwait(false);
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

        private async Task<Universe> LoadUniverseFileAsync()
        {
            using (var stream = File.OpenText(_universePath))
            {
                var fileText = await stream.ReadToEndAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<Universe>(fileText);
            }
        }

        private async Task SaveUniverseFileAsync()
        {
            var fileText = JsonConvert.SerializeObject(_universe);
            using (var stream = File.CreateText(_universePath))
            {
                await stream.WriteAsync(fileText).ConfigureAwait(false);
            }
        }
    }
}