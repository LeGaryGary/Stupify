using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;
using TicTacZap;
using TicTacZap.Segment.Blocks;

using DBSegment = StupifyConsoleApp.DataModels.Segment;
using Segment = TicTacZap.Segment.Segment;

namespace StupifyConsoleApp.AI
{
    class AIController
    {
        private User _user;
        private DBSegment _dbSeg;
        private Segment _seg;
        private BotContext _db;

        public IBlock[,] Blocks
        {
            get { return (IBlock[,]) _seg.Blocks.Clone(); }
        }

        public AIController(BotContext db, DBSegment segment, User user)
        {
            _user = user;
            _dbSeg = segment;
            _db = db;
            _seg = Segments.GetAsync(segment.SegmentId).GetAwaiter().GetResult();
        }

        public async Task UpdateDb()
        {
            await _db.SaveChangesAsync();
            _dbSeg.UnitsPerTick = TicTacZapExtensions.ResourcePerTick(_seg).GetValueOrDefault(Resource.Unit);
            _dbSeg.EnergyPerTick = TicTacZapExtensions.ResourcePerTick(_seg).GetValueOrDefault(Resource.Energy);
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
