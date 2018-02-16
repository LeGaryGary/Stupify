using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Discord;

using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;
using StupifyConsoleApp.AI;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp.Commands
{
    public class AIModule : ModuleBase<SocketCommandContext>
    {
        private readonly string _selectSegmentMessage = $"Please select a segment with {Config.CommandPrefix}segment [segmentId]";

        private BotContext Db { get; } = new BotContext();

        [Command("solve", RunMode = RunMode.Async)]
        public async Task DebugSolve(int segmentId)
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);
            var segment = await Db.Segments.FirstOrDefaultAsync(s => s.SegmentId == segmentId);
            if (segment == null)
            {
                await ReplyAsync("invalid segment ID");
                return;
            }

            await runAI(Db, segment, user);
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

        private async Task runAI(BotContext db, Segment segment, User user)
        {
            AI.AI aiInstance = new AI.AI(Db, segment, user);
            Task ai = Task.Run(() => aiInstance.run());
            IUserMessage msg = await ReplyAsync("hang on...");
            int i = 1;
            while (!ai.IsCompleted)
            {
                await updateMsg(msg, segment);
                await Task.Delay(1000);
            }

            await updateMsg(msg, segment);
            await ReplyAsync("done.");
        }

        private async Task updateMsg(IUserMessage msg, Segment segment)
        {
                await msg.ModifyAsync(message => message.Content = $"```{TicTacZapController.RenderSegment(segment.SegmentId)}```");
        }

    }
}
