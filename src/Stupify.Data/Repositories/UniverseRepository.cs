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

        public async Task<(int x, int y)?> FindAsync(int segmentId)
        {
            return await _universeController.FindAsync(segmentId);
        }

        public async Task<(int x, int y)?> NewSegmentAsync(int segmentId)
        {
            return await _universeController.NewSegmentAsync(segmentId);
        }

        public async Task<(int x, int y)?> DeleteSegmentAsync(int segmentId)
        {
            return await _universeController.DeleteSegment(segmentId);
        }

        public async Task<int?> GetAdjacentSegmentInDirectionAsync(int segmentId, Direction direction)
        {
            return await _universeController.GetAdjacentSegmentInDirectionAsync(segmentId, direction);
        }

        public async Task<string> RenderRelativeToSegmentAsync(int segmentId, int scope)
        {
            return await _universeController.RenderRelativeToSegmentAsync(segmentId, scope);
        }
    }
}
