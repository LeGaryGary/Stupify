using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Stupify.Data.SQL.Models;

namespace Stupify.Data.Repositories
{
    public interface ISettingsRepository
    {
        Task<Models.ServerSettings> GetServerSettingsAsync(ulong discordGuildId);
        Task SetServerSettingsAsync(ulong discordGuildId, Models.ServerSettings serverSettings);
    }
}
