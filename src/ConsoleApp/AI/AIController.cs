using System.Threading.Tasks;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap;
using TicTacZap.Blocks;
using DBSegment = StupifyConsoleApp.DataModels.Segment;
using TicTacZapSegment = TicTacZap.Segment;

namespace StupifyConsoleApp.AI
{
    internal class AIController
    {
        private readonly BotContext _db;
        private readonly DBSegment _dbSeg;
        private readonly TicTacZapSegment _seg;
        private User _user;

        public AIController(BotContext db, DBSegment segment, User user)
        {
            _user = user;
            _dbSeg = segment;
            _db = db;
            _seg = Segments.GetAsync(segment.SegmentId).GetAwaiter().GetResult();
        }

        public IBlock[,] Blocks => (IBlock[,]) _seg.Blocks.Clone();

        public async Task UpdateDb()
        {
            var outputs = await Segments.GetOutput(_dbSeg.SegmentId);
            _dbSeg.EnergyPerTick = outputs[Resource.Energy];
            await _db.SaveChangesAsync();
        }

        public async Task AddBlock(int x, int y)
        {
            await Segments.AddBlockAsync(_dbSeg.SegmentId, x, y, BlockType.Energy);
        }

        public async Task RemoveBlock(int x, int y)
        {
            await Segments.DeleteBlockAsync(_dbSeg.SegmentId, x, y);
        }

        public decimal Output()
        {
            return _seg.ResourcePerTick(Resource.Energy);
        }
    }
}