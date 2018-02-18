using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Discord.Commands;

using StupifyConsoleApp.DataModels;
using TicTacZap;

namespace StupifyConsoleApp.Commands
{
    public static class CommonFunctions
    {
        public static async Task<User> GetUserAsync(BotContext db, SocketCommandContext context)
        {
            return await db.Users.FirstAsync(u => u.DiscordUserId == (long)context.User.Id);
        }

        public static async Task<IEnumerable<Segment>> GetSegments(BotContext db, SocketCommandContext context)
        {
            var user = await GetUserAsync(db, context);
            return db.Segments.Where(s => s.UserId == user.UserId);
        }

        public static async Task<bool> UserHasSegmentAsync(BotContext db, SocketCommandContext context, int segmentId)
        {
            return (await GetSegments(db, context)).Select(s => s.SegmentId).Contains(segmentId);
        }

        public static string NotEnoughUnits(decimal price)
        {
            return $"Come back when you have more money (you need {price} {Resource.Unit} to buy this)";
        }
    }
}
