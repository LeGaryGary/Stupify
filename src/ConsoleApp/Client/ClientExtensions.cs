using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Client
{
    public static class ClientExtensions
    {
        public static async Task<string> UsernameFromServerUser(this IDiscordClient client, ServerUser serverUser)
        {
            var guild = await client.GetGuildAsync((ulong) serverUser.Server.DiscordGuildId);
            var user = await guild.GetUserAsync((ulong) serverUser.User.DiscordUserId);
            return user.Username;
        }
    }
}