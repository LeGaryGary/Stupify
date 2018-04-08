using System.Collections.Generic;
using System.Threading.Tasks;
using Stupify.Data.FileSystem;
using TicTacZap;

namespace Stupify.Data.Repositories
{
    internal class UniverseRepository : IUniverseRepository 
    {
        private readonly UniverseController _universeController;

        public UniverseRepository(UniverseController universeController)
        {
            _universeController = universeController;
        }

        public Task<(int x, int y)?> FindAsync(int segmentId)
        {
            return _universeController.FindAsync(segmentId);
        }

        public Task<(int x, int y)?> NewSegmentAsync(int segmentId)
        {
            return _universeController.NewSegmentAsync(segmentId);
        }

        public Task<(int x, int y)?> DeleteSegmentAsync(int segmentId)
        {
            return _universeController.DeleteSegmentAsync(segmentId);
        }

        public Task<int?> GetAdjacentSegmentInDirectionAsync(int segmentId, Direction direction)
        {
            return _universeController.GetAdjacentSegmentInDirectionAsync(segmentId, direction);
        }

        public Task<string> RenderRelativeToSegmentAsync(int segmentId, int scope)
        {
            return _universeController.RenderRelativeToSegmentAsync(segmentId, scope);
        }

        public IEnumerable<int> UniverseSegments()
        {
            return _universeController.UniverseSegments();
        }
    }
}
