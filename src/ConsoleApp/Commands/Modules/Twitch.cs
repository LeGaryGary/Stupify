using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Stupify.Data.Repositories;
using StupifyConsoleApp.Commands.Conditions;
using TwitchApi;

namespace StupifyConsoleApp.Commands.Modules
{
    public class Twitch : ModuleBase<CommandContext>
    {
        private readonly TwitchClient _twitchClient;
        private readonly ITwitchRepository _twitchRepository;

        public Twitch(TwitchClient twitchClient, ITwitchRepository twitchRepository)
        {
            _twitchClient = twitchClient;
            _twitchRepository = twitchRepository;
        }

        [Command("AreTheyLive")]
        public async Task AreTheyLiveAsync(string username)
        {
            var message = await _twitchClient.IsStreamingAsync(username).ConfigureAwait(false) ? "Yup, they are" : "Uhhh, no?";
            await ReplyAsync(message).ConfigureAwait(false);
        }

        [Command("SetUpdateChannel")]
        [Moderator]
        public async Task SetUpdateChannelAsync()
        {
            await _twitchRepository.SetUpdateChannelAsync(Context.Channel as ITextChannel).ConfigureAwait(false);
            await ReplyAsync("This channel will now be used to send twitch updates!").ConfigureAwait(false);
        }

        [Command("AddChannelUpdate")]
        [Moderator]
        public async Task AddTwitchWatchAsync(string twitchLoginName)
        {
            if (await _twitchClient.GetTwitchUserAsync(twitchLoginName).ConfigureAwait(false) == null)
            {
                await ReplyAsync("This user could not be found").ConfigureAwait(false);
                return;
            }
            await _twitchRepository.AddTwitchChannelWatchAsync(Context.Guild, twitchLoginName).ConfigureAwait(false);
            await ReplyAsync("This user has been added!").ConfigureAwait(false);
        }

        [Command("TwitchChannels")]
        public async Task TwitchChannelsAsync()
        {
            var channels = await _twitchRepository.GuildTwitchChannelsAsync(Context.Guild).ConfigureAwait(false);

            if (channels.Length == 0)
            {
                await ReplyAsync("No twitch channels have been added!").ConfigureAwait(false);
                return;
            }

            var message = new StringBuilder();
            foreach (var channel in channels)
            {
                message.Append("- " + channel + Environment.NewLine);
            }

            await ReplyAsync($"```{message}```").ConfigureAwait(false);
        }
    }
}
