using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Discord.Commands;

using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Commands
{
    public static class CommonFunctions
    {
        public static async Task<User> GetUserAsync(BotContext db, SocketCommandContext context)
        {
            return await db.Users.FirstAsync(u => u.DiscordUserId == (long)context.User.Id);
        }
    }
}
