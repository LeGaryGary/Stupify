using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.Models;
using Stupify.Data.SQL;

namespace Stupify.Data.Repositories
{
    internal class SettingsRepository : ISettingsRepository
    {
        private readonly BotContext _botContext;

        public SettingsRepository(BotContext botContext)
        {
            _botContext = botContext;
        }

        public async Task<ServerSettings> GetServerSettingsAsync(ulong discordGuildId)
        {
            var settings = await GetDbServerSettingsAsync(discordGuildId).ConfigureAwait(false);

            return new ServerSettings
            {
                CommandPrefix = settings.CommandPrefix,
                CustomCommandPrefix = settings.CustomCommandPrefix
            };
        }

        public async Task<Dictionary<ulong, ServerSettings>> GetServerSettingsAsync(List<ulong> discordGuildIds)
        {
            var dict = new Dictionary<ulong, ServerSettings>();
            foreach (var discordGuildId in discordGuildIds)
            {
                dict.Add(discordGuildId, await GetServerSettingsAsync(discordGuildId).ConfigureAwait(false));
            }

            return dict;
        }

        public async Task SetServerSettingsAsync(ulong discordGuildId, ServerSettings serverSettings)
        {
            var settings = await GetDbServerSettingsAsync(discordGuildId).ConfigureAwait(false);

            settings.CommandPrefix = serverSettings.CommandPrefix;
            settings.CustomCommandPrefix = serverSettings.CustomCommandPrefix;

            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task<SQL.Models.ServerSettings> GetDbServerSettingsAsync(ulong discordGuildId)
        {
            var guildId = (long) discordGuildId;
            var settings = await _botContext.ServerSettings
                .FirstOrDefaultAsync(ss => ss.Server.DiscordGuildId == guildId).ConfigureAwait(false);

            if (settings == null)
            {
                settings = _botContext.ServerSettings.Add(new SQL.Models.ServerSettings()).Entity;
                settings.Server = await _botContext.Servers.FirstOrDefaultAsync(s => s.DiscordGuildId == guildId).ConfigureAwait(false);
                await _botContext.SaveChangesAsync().ConfigureAwait(false);
            }

            return settings;
        }
    }
}