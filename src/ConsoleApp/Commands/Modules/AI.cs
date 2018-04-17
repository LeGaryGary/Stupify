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
    public class Ai : ModuleBase<CommandContext>
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

        private static readonly Dictionary<int, AI.AI> AiInstances = new Dictionary<int, AI.AI>();

        public Ai(TicTacZapController tacZapController, GameState gameState, ISegmentRepository segmentRepository, IUserRepository userRepository)
        {
            _tacZapController = tacZapController;
            _gameState = gameState;
            _segmentRepository = segmentRepository;
            _userRepository = userRepository;
        }

        [Command(RunMode = RunMode.Async)]
        public async Task SolveAsync(int segmentId, decimal addThr = ConsiderationThreshold,
            decimal rmvThr = RemoveThreshold,
            double exp = ExpansionChance, double rmv = RemoveChance, double brk = BreakChance)
        {
            var userId = await _userRepository.GetUserIdAsync(Context.User).ConfigureAwait(false);
            if (AiInstances.ContainsKey(userId))
            {
                await ReplyAsync(
                    $"you already have an AI instance running, you can stop it using {Config.CommandPrefix} Solve Stop").ConfigureAwait(false);
                return;
            }

            if (!await _segmentRepository.UserHasSegmentAsync(Context.User, segmentId).ConfigureAwait(false))
            {
                await ReplyAsync(Responses.SegmentOwnershipProblem).ConfigureAwait(false);
                return;
            }

            await RunAiAsync(segmentId, userId, addThr, rmvThr, exp, rmv, brk).ConfigureAwait(false);
        }

        [Command]
        public async Task SolveAsync(decimal addThr = ConsiderationThreshold,
            decimal rmvThr = RemoveThreshold, double exp = ExpansionChance, double rmv = RemoveChance, double brk = BreakChance)
        {
            var userId = await _userRepository.GetUserIdAsync(Context.User).ConfigureAwait(false);
            var id = _gameState.GetUserSegmentSelection(userId);

            if (id != null)
                await SolveAsync((int) id, addThr, rmvThr, exp, rmv, brk).ConfigureAwait(false);
            else
                await ReplyAsync(Responses.SelectSegmentMessage).ConfigureAwait(false);
        }

        [Command("Stop")]
        public async Task StopAsync()
        {
            var userId = await _userRepository.GetUserIdAsync(Context.User).ConfigureAwait(false);
            if (!AiInstances.ContainsKey(userId))
            {
                await ReplyAsync("you have no AI instances running").ConfigureAwait(false);
                return;
            }

            var msg = await ReplyAsync("stopping.").ConfigureAwait(false);
            var instance = AiInstances[userId];
            instance.Stop();
            await msg.ModifyAsync(message => message.Content = "stopped.").ConfigureAwait(false);
        }

        private async Task RunAiAsync(int segmentId, int userId, decimal addThr, decimal rmvThr, double exp,
            double rmv, double brk)
        {
            var aiInstance = new AI.AI(_segmentRepository, segmentId);
            AiInstances.Add(userId, aiInstance);

            var ai = Task.Run(() => aiInstance.Run(exp, rmv, addThr, rmvThr, brk));
            var msg = await ReplyAsync("hang on...").ConfigureAwait(false);

            while (!ai.IsCompleted)
            {
                await UpdateMsgAsync(msg, segmentId, aiInstance.Status).ConfigureAwait(false);
                await Task.Delay(2000).ConfigureAwait(false);
            }

            await UpdateMsgAsync(msg, segmentId, aiInstance.Status).ConfigureAwait(false);
        }

        private async Task UpdateMsgAsync(IUserMessage msg, int segmentId, AI.AI.AIStatus status)
        {
            var str = await _tacZapController.RenderSegmentAsync(segmentId).ConfigureAwait(false) + "\n";
            switch (status)
            {
                case AI.AI.AIStatus.Finished:
                    str += "done.";
                    break;
                case AI.AI.AIStatus.Stopped:
                    str += "stopped.";
                    break;
                case AI.AI.AIStatus.Working:
                    str += "working...";
                    break;
            }

            await msg.ModifyAsync(message => message.Content = $"```{str}```").ConfigureAwait(false);
        }
    }
}