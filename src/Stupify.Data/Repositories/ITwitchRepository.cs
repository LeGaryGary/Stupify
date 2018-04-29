using System.Threading.Tasks;
using Discord;
using Stupify.Data.Models;

namespace Stupify.Data.Repositories
{
    public interface ITwitchRepository
    {
        Task<string[]> GuildTwitchChannelsAsync(IGuild guild);
        Task<TwitchChannel[]> AllTwitchUpdateChannelsAsync();
        Task UpdateLastStatusAsync(long guildId, string twitchLoginName, bool isStreaming);
        Task SetUpdateChannelAsync(ITextChannel channel);
        Task AddTwitchChannelWatchAsync(IGuild contextGuild, string twitchLoginName);
    }
}