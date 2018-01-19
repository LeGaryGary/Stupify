using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Client
{
    public static class ClientExtensions
    {
        public static string UsernameFromServerUser(this DiscordSocketClient client, ServerUser quoteServerUser)
        {
            var discordUserId = quoteServerUser.User.DiscordUserId;
            var guild = client.GetGuild(((ulong) quoteServerUser.Server.DiscordGuildId));
            var user = guild.GetUser((ulong) discordUserId);
            return user.Username;
        }
    }
}
