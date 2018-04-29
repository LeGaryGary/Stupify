using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Stupify.Data.Repositories;
using StupifyConsoleApp.Commands.Conditions;

namespace StupifyConsoleApp.Commands.Modules.Moderation
{
    public class RemoveUser : ModuleBase<CommandContext>
    {
        private readonly ICustomTextRepository _customTextRepository;
        private readonly ISettingsRepository _settingsRepository;

        public RemoveUser(ICustomTextRepository customTextRepository, ISettingsRepository settingsRepository)
        {
            _customTextRepository = customTextRepository;
            _settingsRepository = settingsRepository;
        }

        [Command("Ban")]
        [Moderator]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanCommandAsync(IGuildUser userToBan, int daysOfMessagesToDelete, [Remainder]string banReason)
        {
            if (await _settingsRepository.GetBanChannelAsync(Context.Guild.Id).ConfigureAwait(false) != null) return;

            await Context.Guild.AddBanAsync(userToBan, daysOfMessagesToDelete, banReason).ConfigureAwait(false);
            var message = await _customTextRepository.GetBanTextAsync(userToBan, daysOfMessagesToDelete, banReason).ConfigureAwait(false);
            await ReplyAsync(message).ConfigureAwait(false);
        }

        [Command("Kick")]
        [Moderator]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickCommandAsync(IGuildUser userToKick, [Remainder]string kickReason)
        {
            if (!(Context.Channel is ITextChannel channel)) return;

            var kickChannelId = await _settingsRepository.GetKickChannelAsync(Context.Guild.Id).ConfigureAwait(false);

            await userToKick.KickAsync(kickReason).ConfigureAwait(false);
            var message = await _customTextRepository.GetKickTextAsync(userToKick, kickReason).ConfigureAwait(false);

            var kickChannel = kickChannelId == null
                ? channel
                : await Context.Guild.GetTextChannelAsync(kickChannelId.Value).ConfigureAwait(false);

            await kickChannel.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
