using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Stupify.Data.FileSystem;
using Stupify.Data.SQL;
using TicTacZap;
using TicTacZap.Blocks;

namespace Stupify.Data
{
    internal class SegmentRepository : ISegmentRepository
    {
        private readonly BotContext _botContext;
        private readonly FileSegments _segments;

        public SegmentRepository(BotContext botContext, FileSegments segments)
        {
            _botContext = botContext;
            _segments = segments;
        }

        public async Task<Segment> GetSegmentAsync(int segmentId)
        {
            return await _segments.GetAsync(segmentId);
        }

        public async Task SetSegmentAsync(int segmentId, Segment segment)
        {
            await _segments.SetAsync(segmentId, segment);
        }

        public async Task DeleteSegmentAsync(int segmentId)
        {
            await Task.Run(() => _segments.DeleteSegment(segmentId));
        }

        public async Task<bool> AddBlockAsync(int segmentId, int x, int y, BlockType blockType)
        {
            var segment = await _segments.GetAsync(segmentId);
            var addBlockResult = segment.AddBlock(x, y, blockType);
            if (addBlockResult) await _segments.SetAsync(segmentId, segment);
            return addBlockResult;
        }

        public async Task<BlockType?> DeleteBlockAsync(int segmentId, int x, int y)
        {
            var segment = await _segments.GetAsync(segmentId);
            var deleteBlockResult = segment.DeleteBlock(x, y);
            if (deleteBlockResult.HasValue) await _segments.SetAsync(segmentId, segment);
            return deleteBlockResult;
        }

        public async Task<Dictionary<Resource, decimal>> GetOutput(int segmentId)
        {
            return (await _segments.GetAsync(segmentId)).ResourcePerTick();
        }
    }
}
