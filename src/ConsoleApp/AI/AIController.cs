using System.Threading.Tasks;
using Stupify.Data;
using Stupify.Data.Repositories;
using TicTacZap;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.AI
{
    internal class AIController
    {
        private readonly ISegmentRepository _segmentRepository;
        private readonly int _segmentId;
        private Segment _seg;

        public AIController(ISegmentRepository segmentRepository, int segmentId)
        {
            _segmentRepository = segmentRepository;
            _segmentId = segmentId;
            _seg = _segmentRepository.GetSegmentAsync(segmentId).GetAwaiter().GetResult();
        }

        public IBlock[,] Blocks => (IBlock[,]) _seg.Blocks.Clone();

        public async Task AddBlock(int x, int y)
        {
            await _segmentRepository.AddBlockAsync(_segmentId, x, y, BlockType.Energy);
        }

        public async Task RemoveBlock(int x, int y)
        {
            await _segmentRepository.DeleteBlockAsync(_segmentId, x, y);
        }

        public decimal Output()
        {
            return _seg.ResourcePerTick(Resource.Energy);
        }
    }
}