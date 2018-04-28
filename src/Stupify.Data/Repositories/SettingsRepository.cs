using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.Models;
using Stupify.Data.SQL;

namespace Stupify.Data.Repositories
{
    internal class SettingsRepository : ISettingsRepository
    {
        private readonly BotContext _botContext;
        private readonly IDiscordClient _client;

        public SettingsRepository(BotContext botContext, IDiscordClient client)
        {
            _botContext = botContext;
            _client = client;
        }

        public async Task<ServerSettings> GetServerSettingsAsync(ulong discordGuildId)
        {
            var settings = await GetDbServerSettingsAsync(discordGuildId).ConfigureAwait(false);

            return new ServerSettings
            {
                GuildName = (await _client.GetGuildAsync(discordGuildId).ConfigureAwait(false)).Name,
                CommandPrefix = settings.CommandPrefix,
                CustomCommandPrefix = settings.CustomCommandPrefix,
                ModeratorRoleId = (ulong?)settings.ModeratorRoleId,
                WelcomeChannel = (ulong?)settings.WelcomeChannel,
                LeaveChannel = (ulong?)settings.LeaveChannel,
                BanChannel = (ulong?)settings.BanChannel,
                KickChannel = (ulong?)settings.KickChannel,
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
            settings.ModeratorRoleId = (long?)serverSettings.ModeratorRoleId;
            settings.WelcomeChannel = (long?)serverSettings.WelcomeChannel;
            settings.LeaveChannel = (long?)serverSettings.LeaveChannel;
            settings.BanChannel = (long?)serverSettings.BanChannel;
            settings.KickChannel =(long?)serverSettings.KickChannel;

            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<ulong?> GetWelcomeChannelAsync(ulong guildId)
        {
            var serverSettings = await GetServerSettingsAsync(guildId).ConfigureAwait(false);
            return serverSettings.WelcomeChannel;
        }

        public async Task<ulong?> GetLeaveChannelAsync(ulong guildId)
        {
            var serverSettings = await GetServerSettingsAsync(guildId).ConfigureAwait(false);
            return serverSettings.LeaveChannel;
        }

        public async Task<ulong?> GetBanChannelAsync(ulong guildId)
        {
            var serverSettings = await GetServerSettingsAsync(guildId).ConfigureAwait(false);
            return serverSettings.BanChannel;
        }

        public async Task<ulong?> GetKickChannelAsync(ulong guildId)
        {
            var serverSettings = await GetServerSettingsAsync(guildId).ConfigureAwait(false);
            return serverSettings.KickChannel;
        }

        private async Task<SQL.Models.ServerSettings> GetDbServerSettingsAsync(ulong discordGuildId)
        {
            var guildId = (long) discordGuildId;
            var settings = await _botContext.ServerSettings
                .FirstOrDefaultAsync(ss => ss.Server.DiscordGuildId == guildId).ConfigureAwait(false);

            if (settings != null) return settings;

            settings = _botContext.ServerSettings.Add(new SQL.Models.ServerSettings()).Entity;
            settings.Server = await _botContext.Servers.FirstOrDefaultAsync(s => s.DiscordGuildId == guildId).ConfigureAwait(false);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return settings;
        }
    }
}