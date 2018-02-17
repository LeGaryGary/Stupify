﻿using System.Collections.Generic;
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
        private readonly DBSegment _dbSeg;
        private readonly Segment _seg;
        private readonly BotContext _db;

        public IBlock[,] Blocks => (IBlock[,]) _seg.Blocks.Clone();

        public AIController(BotContext db, DBSegment segment, User user)
        {
            _user = user;
            _dbSeg = segment;
            _db = db;
            _seg = TicTacZapController.GetSegment(segment.SegmentId);
        }

        public async Task UpdateDB()
        {
            await _db.SaveChangesAsync();
            _dbSeg.UnitsPerTick = _seg.ResourcePerTick().GetValueOrDefault(Resource.Unit);
            _dbSeg.EnergyPerTick = _seg.ResourcePerTick().GetValueOrDefault(Resource.Energy);
        }

        public async Task AddBlock(int x, int y)
        {
            await TicTacZapController.AddBlock(_dbSeg.SegmentId, x, y, BlockType.BasicEnergy);
        }

        public async Task RemoveBlock(int x, int y)
        {
            await TicTacZapController.DeleteBlockAsync(_dbSeg.SegmentId, x, y);
        }

        public decimal Output()
        {
            return _seg.ResourcePerTick().GetValueOrDefault(Resource.Energy);
        }

    }
}
