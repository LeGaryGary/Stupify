using System.Collections.Generic;
using System.Threading.Tasks;
using TicTacZap;

namespace Stupify.Data.Repositories
{
    public interface IUniverseRepository
    {
        Task<(int x, int y)?> FindAsync(int segmentId);
        Task<(int x, int y)?> NewSegmentAsync(int segmentId);
        Task<(int x, int y)?> DeleteSegmentAsync(int segmentId);
        Task<int?> GetAdjacentSegmentInDirectionAsync(int segmentValue, Direction direction);
        Task<string>  RenderRelativeToSegmentAsync(int segmentId, int scope);
        IEnumerable<int> UniverseSegments();
    }
}
