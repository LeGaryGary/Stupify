using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;

using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;

namespace StupifyConsoleApp.Commands
{
    class AIModule : ModuleBase<SocketCommandContext>
    {
        private readonly string _selectSegmentMessage = $"Please select a segment with {Config.CommandPrefix}segment [segmentId]";

        private BotContext Db { get; } = new BotContext();

        [Command("solve")]
        public async Task DebugSolve(int segmentId)
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);
            var segment = Db.Segments.First(s => s.SegmentId == segmentId);
            if (segment == null)
            {
                await ReplyAsync("invalid segment ID");
                return;
            }

            await ReplyAsync("WIP");
            // TODO: run AI

        }

        [Command("solve")]
        public async Task DebugSolve()
        {
            User user = await CommonFunctions.GetUserAsync(Db, Context);
            var id = TicTacZapController.GetUserSelection(user.UserId);

            if (id != null)
            {
                await DebugSolve((int)id);
            }
            else
            {
                await ReplyAsync(_selectSegmentMessage);
            }
        }

    }
}
