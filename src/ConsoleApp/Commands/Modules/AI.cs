using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.Commands.Conditions;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Commands.Modules
{
    [Debug]
    public class AI : StupifyModuleBase
    {
        private const decimal ConsiderationThreshold = 30;
        private const double ExpansionChance = 0.1;
        private const double BreakChance = 0.2;

        [Command("Solve", RunMode = RunMode.Async)]
        public async Task Solve(int segmentId, decimal thr = ConsiderationThreshold, double exp = ExpansionChance,
            double brk = BreakChance)
        {
            var user = await this.GetUserAsync();
            if (!await this.UserHasSegmentAsync(segmentId))
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

            await RunAI(Db, segment, user, thr, exp, brk);
        }

        [Command("Solve")]
        public async Task Solve(decimal thr = ConsiderationThreshold, double exp = ExpansionChance,
            double brk = BreakChance)
        {
            var user = await this.GetUserAsync();
            var id = TicTacZapController.GetUserSegmentSelection(user.UserId);

            if (id != null)
                await Solve((int) id, thr, exp, brk);
            else
                await ReplyAsync(Responses.SelectSegmentMessage);
        }

        private async Task RunAI(BotContext db, Segment segment, User user, decimal thr, double exp, double brk)
        {
            var aiInstance = new StupifyConsoleApp.AI.AI(Db, segment, user);
            var ai = Task.Run(() => aiInstance.Run(exp, thr, brk));
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
            str += done ? "done." : "working...";

            await msg.ModifyAsync(message => message.Content = $"```{str}```");
        }
    }
}