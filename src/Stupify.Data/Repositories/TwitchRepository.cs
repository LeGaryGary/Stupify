using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.Models;
using Stupify.Data.SQL;
using Stupify.Data.SQL.Models;

namespace Stupify.Data.Repositories
{
    internal class TwitchRepository : ITwitchRepository
    {
        private readonly BotContext _botContext;

        public TwitchRepository(BotContext botContext)
        {
            _botContext = botContext;
        }

        public Task<string[]> GuildTwitchChannelsAsync(IGuild guild)
        {
            var guildId = (long)guild.Id;
            return _botContext.ServerTwitchChannels.Where(channel => channel.Server.DiscordGuildId == guildId).Select(channel => channel.TwitchLoginName).ToArrayAsync();
        }

        public async Task<TwitchChannel[]> AllTwitchUpdateChannelsAsync()
        {
            var serverTwitchWatchChannels = await _botContext.ServerTwitchChannels
                .Include(channel => channel.Server)
                .Where(channel => channel.Server.TwitchUpdateChannel != null)
                .ToArrayAsync().ConfigureAwait(false);
            return serverTwitchWatchChannels.Select(channel => new TwitchChannel
            {
                GuildId = channel.Server.DiscordGuildId,
                TwitchLoginName = channel.TwitchLoginName,
                LastStatus = channel.LastStatus,
                UpdateChannel = channel.Server.TwitchUpdateChannel.Value
            }).ToArray();
        }

        public async Task UpdateLastStatusAsync(long guildId, string twitchLoginName, bool isStreaming)
        {
            var twitchChannel = await _botContext.ServerTwitchChannels
                .Where(channel => channel.Server.DiscordGuildId == guildId)
                .SingleAsync(channel => channel.TwitchLoginName == twitchLoginName).ConfigureAwait(false);
            twitchChannel.LastStatus = isStreaming;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task SetUpdateChannelAsync(ITextChannel channel)
        {
            var guildId = (long) channel.GuildId;
            var server = await  _botContext.Servers.SingleAsync(s => s.DiscordGuildId == guildId).ConfigureAwait(false);
            server.TwitchUpdateChannel = (long)channel.Id;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task AddTwitchChannelWatchAsync(IGuild guild, string twitchLoginName)
        {
            var guildId = (long) guild.Id;
            var server = await  _botContext.Servers.SingleAsync(s => s.DiscordGuildId == guildId).ConfigureAwait(false);
            _botContext.ServerTwitchChannels.Add(new ServerTwitchChannel
            {
                Server = server,
                TwitchLoginName = twitchLoginName,
                LastStatus = false
            });
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
