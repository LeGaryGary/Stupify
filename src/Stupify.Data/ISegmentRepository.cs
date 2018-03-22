using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicTacZap;
using TicTacZap.Blocks;

namespace Stupify.Data
{
    public interface ISegmentRepository
    {
        Task<Segment> GetSegmentAsync(int segmentId);
        Task SetSegmentAsync(int segmentId, Segment segment);
        Task DeleteSegmentAsync(int segmentId);
        Task<bool> AddBlockAsync(int segmentId, int x, int y, BlockType blockType);
        Task<BlockType?> DeleteBlockAsync(int segmentId, int x, int y);
        Task<Dictionary<Resource, decimal>> GetOutput(int segmentId);
    }
}
