using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace StupifyConsoleApp.Commands.Modules
{
    public class MockingBird : ModuleBase<CommandContext>
    {
        [Command("Mock")]
        public async Task MockCommandAsync(IGuildUser userToMock, [Remainder]string messageToSay)
        {
            var selfGuildUser = await Context.Guild.GetCurrentUserAsync().ConfigureAwait(false);
            var mockNickname = userToMock.Nickname ?? userToMock.Username;
            await selfGuildUser.ModifyAsync(props => props.Nickname = mockNickname).ConfigureAwait(false);
            await ReplyAsync(messageToSay).ConfigureAwait(false);
            await selfGuildUser.ModifyAsync(props => props.Nickname = string.Empty).ConfigureAwait(false);
        }
    }
}
