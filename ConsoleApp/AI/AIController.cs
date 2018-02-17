using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;
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
            _seg = TicTacZapController.GetSegment(_dbSeg.SegmentId);
        }

        public async Task updateDB()
        {
            await _db.SaveChangesAsync();
        }

        public async Task addBlock(int x, int y)
        {
            await TicTacZapController.AddBlock(_dbSeg.SegmentId, x, y, BlockType.Basic);
        }

        public async Task removeBlock(int x, int y)
        {
            await TicTacZapController.DeleteBlockAsync(_dbSeg.SegmentId, x, y);
        }

        public decimal output()
        {
            return _seg.OutputPerTick;
        }
    }
}
