using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stupify.Data.Repositories
{
    public interface ISettingsRepository
    {
        Task<Models.ServerSettings> GetServerSettingsAsync(ulong discordGuildId);
        Task<Dictionary<ulong, Models.ServerSettings>> GetServerSettingsAsync(List<ulong> discordGuildId);
        Task SetServerSettingsAsync(ulong discordGuildId, Models.ServerSettings serverSettings);
    }
}
