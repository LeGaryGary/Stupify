using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stupify.Data.Repositories
{
    public interface ISettingsRepository
    {
        Task<Models.ServerSettings> GetServerSettingsAsync(ulong discordGuildId);
        Task<Dictionary<ulong, Models.ServerSettings>> GetServerSettingsAsync(List<ulong> discordGuildId);
        Task SetServerSettingsAsync(ulong discordGuildId, Models.ServerSettings serverSettings);

        Task<ulong?> GetWelcomeChannelAsync(ulong guildId);
        Task<ulong?> GetLeaveChannelAsync(ulong guildId);
        Task<ulong?> GetBanChannelAsync(ulong guildId);
        Task<ulong?> GetKickChannelAsync(ulong guildId);
    }
}
