using System.Threading.Tasks;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap;
using TicTacZap.Segment.Blocks;
using DBSegment = StupifyConsoleApp.DataModels.Segment;
using Segment = TicTacZap.Segment.Segment;

namespace StupifyConsoleApp.AI
{
    internal class AIController
    {
        private readonly BotContext _db;
        private readonly DBSegment _dbSeg;
        private readonly Segment _seg;
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
            _dbSeg.UnitsPerTick = outputs[Resource.Unit];
            _dbSeg.EnergyPerTick = outputs[Resource.Energy];
            await _db.SaveChangesAsync();
        }

        public async Task AddBlock(int x, int y)
        {
            await Segments.AddBlockAsync(_dbSeg.SegmentId, x, y, BlockType.BasicEnergy);
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