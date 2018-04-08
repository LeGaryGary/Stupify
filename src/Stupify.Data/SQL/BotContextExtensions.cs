using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.SQL.Models;
using TicTacZap;

namespace Stupify.Data.SQL
{
    internal static class BotContextExtensions
    {
        public static Task<Dictionary<Resource, decimal>> GetSegmentResourcesAsync(this BotContext context, int segmentId)
        {
            var dict = new Dictionary<Resource, decimal>();
            return Task.FromResult(dict);
        }

        public static async Task<Dictionary<Resource, decimal>> GetSegmentResourcePerTickAsync(this BotContext context, int segmentId)
        {
            var dict = new Dictionary<Resource, decimal>();
            var segment = await context.Segments.FirstAsync(s => s.SegmentId == segmentId).ConfigureAwait(false);

            dict.Add(Resource.Energy, segment.EnergyPerTick);
            dict.Add(Resource.Unit, segment.UnitsPerTick);

            return dict;
        }

        public static async Task<int> NewTemplateAsync(this BotContext context, User user)
        {
            var segmentTemplate = new SegmentTemplate
            {
                User = user
            };
            context.SegmentTemplates.Add(segmentTemplate);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return segmentTemplate.SegmentTemplateId;
        }

        public static async Task<User> GetBankAsync(this BotContext db)
        {
            var bankUser = await db.Users.FirstOrDefaultAsync(u => u.DiscordUserId == -1).ConfigureAwait(false)
                           ?? db.Users.Add(new User
                           {
                               Balance = 100000000,
                               DiscordUserId = -1,
                           }).Entity;

            await db.SaveChangesAsync().ConfigureAwait(false);
            return bankUser;
        }
    }
}