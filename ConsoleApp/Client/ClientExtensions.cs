using Discord.WebSocket;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Client
{
    public static class ClientExtensions
    {
        public static string UsernameFromServerUser(this DiscordSocketClient client, ServerUser serverUser)
        {
            var guild = client.GetGuild((ulong) serverUser.Server.DiscordGuildId);
            var user = guild.GetUser((ulong) serverUser.User.DiscordUserId);
            return user.Username;
        }
    }
}
