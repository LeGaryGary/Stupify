using System.Threading.Tasks;
using Discord.Commands;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class Universe : StupifyModuleBase
    {
        private readonly GameState _gameState;

        public Universe(BotContext db, GameState gameState) : base(db)
        {
            _gameState = gameState;
        }

        [Command("Universe")]
        public async Task ShowUniverseCommand()
        {
            await ShowUniverseCommand(10);
        }

        [Command("Universe")]
        public async Task ShowUniverseCommand(int scope)
        {
            var userSelection = _gameState.GetUserSegmentSelection((await this.GetUserAsync()).UserId);
            if (!userSelection.HasValue)
            {
                await ReplyAsync(Responses.SelectSegmentMessage);
                return;
            }

            var renderRelativeToSegment = UniverseController.RenderRelativeToSegment(userSelection.Value, scope);
            if (string.IsNullOrEmpty(renderRelativeToSegment))
            {
                await ReplyAsync("There's nothing to show! (or you felt like a smart cookie and tried a scope bigger than 12!)");
                return;
            }
            await ReplyAsync($"```{renderRelativeToSegment}```");
        }
    }
}