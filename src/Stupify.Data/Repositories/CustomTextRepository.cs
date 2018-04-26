using System;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.Models;
using Stupify.Data.SQL;
using Stupify.Data.SQL.Models;

namespace Stupify.Data.Repositories
{
    internal class CustomTextRepository : ICustomTextRepository
    {
        private readonly BotContext _botContext;

        public CustomTextRepository(BotContext botContext)
        {
            _botContext = botContext;
        }

        public async Task<string> GetBanTextAsync(IGuildUser userToBan, int? daysOfMessagesToDelete, string banReason)
        {
            var text = (await GetServerCustomTextAsync(userToBan.GuildId, CustomText.Ban).ConfigureAwait(false)).Text;

            if (text == null) return $"```Banned!{Environment.NewLine}User: {userToBan.Username}{userToBan.Discriminator}{Environment.NewLine}Deleted messages from last {daysOfMessagesToDelete ?? 0} days{Environment.NewLine}Reason: {banReason}```";

            text = text.Replace("{USERNAME}", userToBan.Username + userToBan.Discriminator);
            text = text.Replace("{DAYS_OF_MESSAGES_DELETED}", (daysOfMessagesToDelete ?? 0).ToString());
            text = text.Replace("{BAN_REASON}", banReason);

            return text;
        }

        public async Task SetBanTextAsync(ulong guildId, string banText)
        {
            var serverText = await GetServerCustomTextAsync(guildId, CustomText.Ban).ConfigureAwait(false);
            serverText.Text = banText;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<string> GetKickTextAsync(IGuildUser userToKick, string kickReason)
        {
            var text = (await GetServerCustomTextAsync(userToKick.GuildId, CustomText.Kick).ConfigureAwait(false)).Text;

            if (text == null) return $"```Kicked!{Environment.NewLine}User: {userToKick.Username}{userToKick.Discriminator}{Environment.NewLine}Reason: {kickReason}```";

            text = text.Replace("{USERNAME}", userToKick.Username + userToKick.Discriminator);
            text = text.Replace("{KICK_REASON}", kickReason);

            return text;
        }

        public async Task SetKickTextAsync(ulong guildId, string kickText)
        {
            var serverText = await GetServerCustomTextAsync(guildId, CustomText.Kick).ConfigureAwait(false);
            serverText.Text = kickText;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<string> GetWelcomeTextAsync(IGuildUser userThatJoined)
        {
            var text = (await GetServerCustomTextAsync(userThatJoined.GuildId, CustomText.Welcome).ConfigureAwait(false)).Text;

            if (text == null) return $"Welcome {userThatJoined.Username}!";

            text = text.Replace("{USERNAME}", userThatJoined.Username);

            return text;
        }

        public async Task SetWelcomeTextAsync(ulong guildId, string welcomeText)
        {
            var serverText = await GetServerCustomTextAsync(guildId, CustomText.Welcome).ConfigureAwait(false);
            serverText.Text = welcomeText;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<string> GetLeaveTextAsync(IGuildUser userThatLeft)
        {
            var text = (await GetServerCustomTextAsync(userThatLeft.GuildId, CustomText.Leave).ConfigureAwait(false)).Text;

            if (text == null) return $"{userThatLeft.Username} has left";

            text = text.Replace("{USERNAME}", userThatLeft.Username);

            return text;
        }

        public async Task SetLeaveTextAsync(ulong guildId, string leaveText)
        {
            var serverText = await GetServerCustomTextAsync(guildId, CustomText.Leave).ConfigureAwait(false);
            serverText.Text = leaveText;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task<ServerCustomText> GetServerCustomTextAsync(ulong discordGuildId, CustomText type)
        {
            var guildId = (long)discordGuildId;
            var serverText = await _botContext.ServerCustomTexts
                .FirstOrDefaultAsync(sct =>sct.Server.DiscordGuildId == guildId 
                                           && sct.Type == type).ConfigureAwait(false);

            if (serverText != null) return serverText;
            
            var server = await _botContext.Servers.FirstOrDefaultAsync(s => s.DiscordGuildId == guildId).ConfigureAwait(false);
            serverText = new ServerCustomText
            {
                Server = server,
                Type = type
            };
            _botContext.ServerCustomTexts.Add(serverText);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return serverText;
        }
    }
}