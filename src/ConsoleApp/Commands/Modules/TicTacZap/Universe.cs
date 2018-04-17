using System.Threading.Tasks;
using Discord.Commands;
using Stupify.Data.Repositories;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class Universe : ModuleBase<CommandContext>
    {
        private readonly GameState _gameState;
        private readonly IUserRepository _userRepository;
        private readonly IUniverseRepository _universeRepository;

        public Universe(GameState gameState, IUserRepository userRepository, IUniverseRepository universeRepository)
        {
            _gameState = gameState;
            _userRepository = userRepository;
            _universeRepository = universeRepository;
        }

        [Command("Universe")]
        public Task ShowUniverseCommandAsync()
        {
            return ShowUniverseCommandAsync(10);
        }

        [Command("Universe")]
        public async Task ShowUniverseCommandAsync(int scope)
        {
            var userId = await _userRepository.GetUserIdAsync(Context.User).ConfigureAwait(false);
            var userSelection = _gameState.GetUserSegmentSelection(userId);
            if (!userSelection.HasValue)
            {
                await ReplyAsync(Responses.SelectSegmentMessage).ConfigureAwait(false);
                return;
            }

            var renderRelativeToSegment = await _universeRepository.RenderRelativeToSegmentAsync(userSelection.Value, scope).ConfigureAwait(false);
            if (string.IsNullOrEmpty(renderRelativeToSegment))
            {
                await ReplyAsync("There's nothing to show! (or you felt like a smart cookie and tried a scope bigger than 12!)").ConfigureAwait(false);
                return;
            }
            await ReplyAsync($"```{renderRelativeToSegment}```").ConfigureAwait(false);
        }
    }
}