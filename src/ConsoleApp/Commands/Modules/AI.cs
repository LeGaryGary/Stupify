using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Stupify.Data.Repositories;
using StupifyConsoleApp.Commands.Conditions;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Commands.Modules
{
    [Debug]
    [Group("Solve")]
    public class AI : ModuleBase<CommandContext>
    {
        private readonly TicTacZapController _tacZapController;
        private readonly GameState _gameState;
        private readonly ISegmentRepository _segmentRepository;
        private readonly IUserRepository _userRepository;

        private const decimal ConsiderationThreshold = 30;
        private const decimal RemoveThreshold = 20;
        private const double ExpansionChance = 0.1;
        private const double RemoveChance = 0.05;
        private const double BreakChance = 0.2;

        private static readonly Dictionary<int, StupifyConsoleApp.AI.AI> AiInstances = new Dictionary<int, StupifyConsoleApp.AI.AI>();

        public AI(TicTacZapController tacZapController, GameState gameState, ISegmentRepository segmentRepository, IUserRepository userRepository)
        {
            _tacZapController = tacZapController;
            _gameState = gameState;
            _segmentRepository = segmentRepository;
            _userRepository = userRepository;
        }

        [Command(RunMode = RunMode.Async)]
        public async Task Solve(int segmentId, decimal addThr = ConsiderationThreshold,
            decimal rmvThr = RemoveThreshold,
            double exp = ExpansionChance, double rmv = RemoveChance, double brk = BreakChance)
        {
            var userId = await _userRepository.GetUserId(Context.User);
            if (AiInstances.ContainsKey(userId))
            {
                await ReplyAsync(
                    $"you already have an AI instance running, you can stop it using {Config.CommandPrefix} Solve Stop");
                return;
            }

            if (!await _segmentRepository.UserHasSegmentAsync(Context.User, segmentId))
            {
                await ReplyAsync(Responses.SegmentOwnershipProblem);
                return;
            }

            await RunAI(segmentId, userId, addThr, rmvThr, exp, rmv, brk);
        }

        [Command]
        public async Task Solve(decimal addThr = ConsiderationThreshold,
            decimal rmvThr = RemoveThreshold, double exp = ExpansionChance, double rmv = RemoveChance, double brk = BreakChance)
        {
            var userId = await _userRepository.GetUserId(Context.User);
            var id = _gameState.GetUserSegmentSelection(userId);

            if (id != null)
                await Solve((int) id, addThr, rmvThr, exp, rmv, brk);
            else
                await ReplyAsync(Responses.SelectSegmentMessage);
        }

        [Command("Stop")]
        public async Task Stop()
        {
            var userId = await _userRepository.GetUserId(Context.User);
            if (!AiInstances.ContainsKey(userId))
            {
                await ReplyAsync("you have no AI instances running");
                return;
            }

            var msg = await ReplyAsync("stopping.");
            var instance = AiInstances[userId];
            instance.Stop();
            await msg.ModifyAsync(message => message.Content = "stopped.");
        }

        private async Task RunAI(int segmentId, int userId, decimal addThr, decimal rmvThr, double exp,
            double rmv, double brk)
        {
            var aiInstance = new StupifyConsoleApp.AI.AI(_segmentRepository, segmentId);
            AiInstances.Add(userId, aiInstance);

            var ai = Task.Run(() => aiInstance.Run(exp, rmv, addThr, rmvThr, brk));
            var msg = await ReplyAsync("hang on...");

            while (!ai.IsCompleted)
            {
                await UpdateMsg(msg, segmentId, aiInstance.Status);
                await Task.Delay(2000);
            }

            await UpdateMsg(msg, segmentId, aiInstance.Status);
        }

        private async Task UpdateMsg(IUserMessage msg, int segmentId, StupifyConsoleApp.AI.AI.AIStatus status)
        {
            var str = await _tacZapController.RenderSegmentAsync(segmentId) + "\n";
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