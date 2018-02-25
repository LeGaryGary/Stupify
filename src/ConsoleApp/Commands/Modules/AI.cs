using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Commands.Modules
{
    [Group("Solve")]
    public class AI : StupifyModuleBase
    {
        private const decimal ConsiderationThreshold = 30;
        private const decimal RemoveThreshold = 20;
        private const double ExpansionChance = 0.1;
        private const double RemoveChance = 0.05;
        private const double BreakChance = 0.2;

        private static readonly Dictionary<int, StupifyConsoleApp.AI.AI> AiInstances = new Dictionary<int, StupifyConsoleApp.AI.AI>();

        [Command(RunMode = RunMode.Async)]
        public async Task Solve(int segmentId, decimal addThr = ConsiderationThreshold,
            decimal rmvThr = RemoveThreshold,
            double exp = ExpansionChance, double rmv = RemoveChance, double brk = BreakChance)
        {
            var user = await this.GetUserAsync();
            if (AiInstances.ContainsKey(user.UserId))
            {
                await ReplyAsync(
                    $"you already have an AI instance running, you can stop it using {Config.CommandPrefix} Solve Stop");
                return;
            }

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

            await RunAI(Db, segment, user, addThr, rmvThr, exp, rmv, brk);
        }

        [Command]
        public async Task Solve(decimal addThr = ConsiderationThreshold, decimal rmvThr = RemoveThreshold,
            double exp = ExpansionChance, double rmv = RemoveChance, double brk = BreakChance)
        {
            var user = await this.GetUserAsync();
            var id = TicTacZapController.GetUserSelection(user.UserId);

            if (id != null)
                await Solve((int) id, addThr, rmvThr, exp, rmv, brk);
            else
                await ReplyAsync(Responses.SelectSegmentMessage);
        }

        [Command("Stop")]
        public async Task Stop()
        {
            var user = await this.GetUserAsync();
            if (!AiInstances.ContainsKey(user.UserId))
            {
                await ReplyAsync("you have no AI instances running");
                return;
            }

            var msg = await ReplyAsync("stopping.");
            var instance = AiInstances[user.UserId];
            instance.Stop();
            await msg.ModifyAsync(message => message.Content = "stopped.");
        }

        private async Task RunAI(BotContext db, Segment segment, User user, decimal addThr, decimal rmvThr, double exp,
            double rmv, double brk)
        {
            var aiInstance = new StupifyConsoleApp.AI.AI(Db, segment, user);
            AiInstances.Add(user.UserId, aiInstance);

            var ai = Task.Run(() => aiInstance.Run(exp, rmv, addThr, rmvThr, brk));
            var msg = await ReplyAsync("hang on...");

            while (!ai.IsCompleted)
            {
                await UpdateMsg(msg, segment, aiInstance.Status);
                await Task.Delay(2000);
            }

            await UpdateMsg(msg, segment, aiInstance.Status);
        }

        private async Task UpdateMsg(IUserMessage msg, Segment segment, StupifyConsoleApp.AI.AI.AIStatus status)
        {
            var str = await TicTacZapController.RenderSegmentAsync(segment.SegmentId, Db) + "\n";
            switch (status)
            {
                case StupifyConsoleApp.AI.AI.AIStatus.Finished:
                    str += "done.";
                    break;
                case StupifyConsoleApp.AI.AI.AIStatus.Stopped:
                    str += "stopped.";
                    break;
                case StupifyConsoleApp.AI.AI.AIStatus.Working:
                    str += "working...";
                    break;
            }

            await msg.ModifyAsync(message => message.Content = $"```{str}```");
        }

    }
}