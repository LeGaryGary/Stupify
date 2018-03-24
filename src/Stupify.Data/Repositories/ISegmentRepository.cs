using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using TicTacZap;
using TicTacZap.Blocks;

namespace Stupify.Data.Repositories
{
    public interface ISegmentRepository
    {
        Task<(int segmentId, (int x, int y)? coords)> NewSegmentAsync(IUser user);
        Task<Segment> GetSegmentAsync(int segmentId);
        Task SetSegmentAsync(int segmentId, Segment segment);
        Task<(int x, int y)?> DeleteSegmentAsync(int segmentId);
        Task<Dictionary<BlockType, int>> ResetSegmentAsync(int segmentId);
        Task<bool> AddBlockAsync(int segmentId, int x, int y, BlockType blockType);
        Task<BlockType?> DeleteBlockAsync(int segmentId, int x, int y);
        Task<Dictionary<Resource, decimal>> GetOutput(int segmentId);
        Task<bool> Exists(int segmentId);
        Task<IUser> GetOwner(int segmentId);
        Task<int> SegmentCountAsync(IUser user);
        Task<IEnumerable<Segment>> GetSegments(IUser user);
        Task<IEnumerable<int>> GetSegmentIds(IUser user);
        Task<bool> UserHasSegmentAsync(IUser user, int segmentId);
        Task UpdateBalancesAsync();
        Task<Dictionary<Resource,decimal>> GetSegmentResourcePerTickAsync(int segmentId);
        Task<Dictionary<Resource,decimal>> GetSegmentResourcesAsync(int segmentId);
    }
}
