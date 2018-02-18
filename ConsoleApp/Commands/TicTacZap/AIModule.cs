using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore
    ;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;

namespace StupifyConsoleApp.Commands.TicTacZap
{
    public class AIModule : ModuleBase<SocketCommandContext>
    {
        private BotContext Db { get; } = new BotContext();

        [Command("solve", RunMode = RunMode.Async)]
        public async Task Solve(int segmentId)
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);
            if (!await CommonFunctions.UserHasSegmentAsync(Db, Context, segmentId))
            {
                await ReplyAsync(Responses.SegmentOwnershipProblem);
                return;
            }
            var segment = await Db.Segments.FirstOrDefaultAsync(s => s.SegmentId == segmentId);
            if (segment == null)
            {
                await ReplyAsync("invalid segment ID");
                return;
            }

            await RunAI(Db, segment, user);
        }

        [Command("solve")]
        public async Task Solve()
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);
            var id = TicTacZapController.GetUserSelection(user.UserId);

            if (id != null)
            {
                await Solve((int)id);
            }
            else
            {
                await ReplyAsync(Responses.SelectSegmentMessage);
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
            var str = await TicTacZapController.RenderSegmentAsync(segment.SegmentId, Db) + "\n";
            str += (done) ? "done." : "working...";

            await msg.ModifyAsync(message => message.Content = $"```{str}```");
        }

    }
}
