using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace StupifyConsoleApp.Commands.Modules
{
    public class MockingBird : ModuleBase<CommandContext>
    {
        [Command("Mock"), Priority(1)]
        [RequireBotPermission(GuildPermission.ChangeNickname)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public Task MockCommandAsync(IGuildUser userToMockTag, [Remainder]string messageToSay)
        {
            var mockNickname = userToMockTag.Nickname ?? userToMockTag.Username;
            return MockAsync(messageToSay, mockNickname);
        }

        [Command("Mock"), Priority(0)]
        public Task MockCommandAsync(string nameToMock, [Remainder]string messageToSay)
        {
            return MockAsync(messageToSay, nameToMock);
        }

        private async Task MockAsync(string messageToSay, string mockNickname)
        {
            var selfGuildUser = await Context.Guild.GetCurrentUserAsync().ConfigureAwait(false);
            await selfGuildUser.ModifyAsync(props => props.Nickname = mockNickname).ConfigureAwait(false);
            await ReplyAsync(messageToSay).ConfigureAwait(false);
            await selfGuildUser.ModifyAsync(props => props.Nickname = string.Empty).ConfigureAwait(false);
        }
    }
}
