using System.Threading.Tasks;
using Discord.Commands;

namespace StupifyConsoleApp.Commands.Modules
{
    [Group("Spotify")]
    public class Spotify : ModuleBase<CommandContext>
    {
        public Spotify()
        {

        }

        [Command("Connect")]
        public async Task SpotifyConnectAsync()
        {
            var channel = await Context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
        }
    }
}
