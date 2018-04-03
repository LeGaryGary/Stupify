using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.SQL.Models;
using TicTacZap;

namespace Stupify.Data.SQL
{
    internal static class BotContextExtensions
    {
        public static async Task<User> GetDbUser(this BotContext context, IUser user)
        {
            var discordId = (long) user.Id;
            return await context.Users.FirstAsync(u => u.DiscordUserId == discordId);
        }

        public static async Task<ServerStory> GetLatestServerStoryAsync(this BotContext context, long serverId)
        {
            var serverStories =
                await context.ServerStories.Where(ss => ss.Server.DiscordGuildId == serverId).ToListAsync();
            var maxSsId = serverStories.Select(x => x.ServerStoryId).Max();
            return context.ServerStories.First(ss => ss.ServerStoryId == maxSsId);
        }

        public static async Task<ServerStoryPart> GetLastestServerStoryPartAsync(
            this BotContext context,
            ServerStory serverStory)
        {
            var serverStoryParts = await context.ServerStoryParts
                .Where(ssp => ssp.ServerStory.ServerStoryId == serverStory.ServerStoryId)
                .ToListAsync();
            var sspId = serverStoryParts.Select(x => x.ServerStoryPartId).Max();
            return await context.ServerStoryParts.FirstAsync(ssp => ssp.ServerStoryPartId == sspId);
        }

        public static async Task<Dictionary<Resource, decimal>> GetSegmentResourcesAsync(this BotContext context, int segmentId)
        {
            var dict = new Dictionary<Resource, decimal>();
            var segment = await context.Segments.FirstAsync(s => s.SegmentId == segmentId);
            
            return dict;
        }

        public static async Task<Dictionary<Resource, decimal>> GetSegmentResourcePerTickAsync(this BotContext context, int segmentId)
        {
            var dict = new Dictionary<Resource, decimal>();
            var segment = await context.Segments.FirstAsync(s => s.SegmentId == segmentId);

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
            await context.SaveChangesAsync();
            return segmentTemplate.SegmentTemplateId;
        }

        public static async Task<User> GetBankAsync(this BotContext db)
        {
            var bankUser = await db.Users.FirstOrDefaultAsync(u => u.DiscordUserId == -1)
                           ?? db.Users.Add(new User
                           {
                               Balance = 100000000,
                               DiscordUserId = -1,
                           }).Entity;

            await db.SaveChangesAsync();
            return bankUser;
        }
    }
}