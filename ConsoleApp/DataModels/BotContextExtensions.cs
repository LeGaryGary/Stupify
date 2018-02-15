using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace StupifyConsoleApp.DataModels
{
    public static class  BotContextExtensions
    {
        public static async Task<ServerUser> GetServerUserAsync(this BotContext context, ulong discordUserId, ulong discordServerId, bool addIfNotFound = false)
        {
            return await context.GetServerUserAsync((long) discordUserId, (long) discordServerId, addIfNotFound);
        }
        public static async Task<ServerUser> GetServerUserAsync(this BotContext context, long discordUserId, long discordServerId,  bool addIfNotFound = false)
        {
            var serverUser = await context.ServerUsers.FirstOrDefaultAsync(
                x => x.Server.DiscordGuildId == discordServerId && x.User.DiscordUserId == discordUserId);

            if (serverUser != null || !addIfNotFound) return serverUser;

            //No ServerUser Exists => create one

            var userTask = context.Users.FirstOrDefaultAsync(
                x => x.DiscordUserId == discordUserId);
            var serverTask = context.Servers.FirstOrDefaultAsync(
                x => x.DiscordGuildId == discordServerId);

            await context.ServerUsers.AddAsync(new ServerUser()
            {
                User = await userTask ?? new User{ DiscordUserId = discordUserId },
                Server = await serverTask ?? new Server{ DiscordGuildId = discordServerId }
            });
            await context.SaveChangesAsync();

            return await context.ServerUsers.FirstOrDefaultAsync(
                       su => su.Server.DiscordGuildId == discordServerId &&
                             su.User.DiscordUserId == discordUserId) 
                   ?? throw new InvalidOperationException("Newly added serveruser not found!");
        }

        public static async Task<ServerStory> GetLatestServerStoryAsync(this BotContext context, long serverId)
        {
            var serverStories = await context.ServerStories.Where(ss => ss.Server.DiscordGuildId == serverId).ToListAsync();
            var maxSsId = serverStories.Select(x => x.ServerStoryId).Max();
            return context.ServerStories.First(ss => ss.ServerStoryId == maxSsId);
        }

        public static async Task<ServerStoryPart> GetLastestServerStoryPartAsync(
            this BotContext context,
            ServerStory serverStory)
        {
            var serverStoryParts = await context.ServerStoryParts.Where(ssp => ssp.ServerStory.ServerStoryId == serverStory.ServerStoryId).ToListAsync();
            var sspId = serverStoryParts.Select(x => x.ServerStoryPartId).Max();
            return await context.ServerStoryParts.FirstAsync(ssp => ssp.ServerStoryPartId == sspId);
        }
    }
}
