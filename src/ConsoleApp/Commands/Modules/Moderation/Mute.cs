using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Stupify.Data;
using Stupify.Data.Repositories;

namespace StupifyConsoleApp.Commands.Modules.Moderation
{
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public class Mute : ModuleBase<CommandContext>
    {
        private readonly IUserRepository _userRepository;

        public Mute(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [Command("Mute")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task MuteAsync(IGuildUser user)
        {
            await _userRepository.MuteAsync(user);
            await ReplyAsync($"{user.Username} is now muted");
        }

        [Command("Unmute")]
        public async Task UnMuteAsync(IGuildUser user)
        {
            var message = await _userRepository.UnMuteAsync(user)
                ? await ReplyAsync($"{user.Username} is no longer muted!")
                : await ReplyAsync($"{user.Username} isn't muted!");
        }
    }
}