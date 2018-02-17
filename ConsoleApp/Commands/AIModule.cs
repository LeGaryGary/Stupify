using Discord.Commands;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Discord;

using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;

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

            await RunAI(Db, segment, user);
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

        private async Task RunAI(BotContext db, Segment segment, User user)
        {
            var aiInstance = new AI.AI(Db, segment, user);
            var ai = Task.Run(() => aiInstance.Run());
            var msg = await ReplyAsync("hang on...");
            while (!ai.IsCompleted)
            {
                await UpdateMsg(msg, segment);
                await Task.Delay(2000);
            }

            await UpdateMsg(msg, segment, true);
        }

        private async Task UpdateMsg(IUserMessage msg, Segment segment, bool done = false)
        {
            var str = TicTacZapController.RenderSegment(segment.SegmentId, Db) + "\n";
            str += (done) ? "done." : "working...";

            await msg.ModifyAsync(message => message.Content = $"```{str}```");
        }

    }
}
