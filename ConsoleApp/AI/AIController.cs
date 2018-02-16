using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;

using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;
using TicTacZap.Segment.Blocks;

namespace StupifyConsoleApp.AI
{
    class AIController : ModuleBase<SocketCommandContext>
    {
        private User _user;
        private Segment _segment;
        private BotContext _db;
        private IUserMessage _msg;

        public AIController(BotContext db, Segment segment, User user)
        {
            _user = user;
            _segment = segment;
            _db = db;
            _msg = null;
        }

        public async Task updateMsg()
        {
            if (_msg == null)
            {
                //_msg = await ReplyAsync($"```{TicTacZapController.RenderSegment(_segment.SegmentId)}```");
                Console.WriteLine("update");
            }
            else
            {
                await _msg.ModifyAsync(msg => msg.Content = $"```{TicTacZapController.RenderSegment(_segment.SegmentId)}```");
            }
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
