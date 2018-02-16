using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;
using TicTacZap.Segment.Blocks;

namespace StupifyConsoleApp.AI
{
    class AIController
    {
        private User _user;
        private Segment _segment;
        private BotContext _db;

        public AIController(BotContext db, Segment segment, User user)
        {
            _user = user;
            _segment = segment;
            _db = db;
        }

        public async Task updateDB()
        {
            await _db.SaveChangesAsync();
        }

        public async Task addBlock(int x, int y)
        {
            await TicTacZapController.AddBlock(_segment.SegmentId, x, y, BlockType.Basic);
        }

        public async Task removeBlock(int x, int y)
        {
            await TicTacZapController.DeleteBlockAsync(_segment.SegmentId, x, y);
        }
    }
}
