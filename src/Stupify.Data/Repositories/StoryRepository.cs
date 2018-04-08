using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.Models;
using Stupify.Data.SQL;
using Stupify.Data.SQL.Models;

namespace Stupify.Data.Repositories
{
    internal class StoryRepository : IStoryRepository
    {
        private const int MinLength = 5;

        private readonly BotContext _botContext;

        public StoryRepository(BotContext botContext)
        {
            _botContext = botContext;
        }

        public async Task<Story> RandomStoryAsync(IGuild guild)
        {
            var dbStory = await _botContext.ServerStories
                .Where(ss => ss.Server.DiscordGuildId == (long) guild.Id)
                .OrderBy(x => new Guid())
                .Include(ss => ss.ServerStoryParts)
                .FirstOrDefaultAsync();

            if (dbStory == null) return null;

            var parts = await _botContext.ServerStoryParts
                .Where( ssp => ssp.ServerStory.ServerStoryId == dbStory.ServerStoryId)
                .ToArrayAsync();

            var story = new Story(MinLength)
            {
                Parts = parts.Select(p => p.Part)
            };
            return story;
        }

        public async Task<Story> GetCurrentStoryAsync(IGuild guild)
        {
            var dbStory = await _botContext.ServerStories
                .Where(ss => ss.Server.DiscordGuildId == (long) guild.Id)
                .Include(ss => ss.ServerStoryParts)
                .FirstOrDefaultAsync(ss => ss.EndTime < ss.StartTime);

            if (dbStory == null) return null;

            var parts = await _botContext.ServerStoryParts
                .Where( ssp => ssp.ServerStory.ServerStoryId == dbStory.ServerStoryId)
                .ToArrayAsync();

            var story = new Story(MinLength)
            {
                Parts = parts.Select(p => p.Part)
            };
            return story;
        }

        public async Task<bool> EndCurrentStoryAsync(IGuild guild)
        {
            var dbStory = await _botContext.ServerStories
                .Where(ss => ss.Server.DiscordGuildId == (long) guild.Id)
                .Include(ss => ss.ServerStoryParts)
                .FirstOrDefaultAsync(ss => ss.EndTime < ss.StartTime);

            if (dbStory == null) return false;

            var parts = await _botContext.ServerStoryParts
                .Where( ssp => ssp.ServerStory.ServerStoryId == dbStory.ServerStoryId)
                .ToArrayAsync();

            var story = new Story(MinLength)
            {
                Parts = parts.Select(p => p.Part)
            };

            if (!story.AtLeastMinLength()) return false;

            dbStory.EndTime = DateTime.UtcNow;
            await _botContext.SaveChangesAsync();

            return true;

        }

        public async Task<bool> AddToCurrentStoryAsync(IGuildUser user, string line)
        {
            var su = await _botContext.ServerUsers
                .FirstOrDefaultAsync(u => u.User.DiscordUserId == (long) user.Id &&
                                          u.Server.DiscordGuildId == (long) user.GuildId);

            var dbStory = await _botContext.ServerStories
                .Where(ss => ss.Server.DiscordGuildId == (long) user.GuildId)
                .Include(ss => ss.ServerStoryParts)
                .FirstOrDefaultAsync(ss => ss.EndTime < ss.StartTime);
            
            if (dbStory == null) return false;

            var lastPart = await _botContext.ServerStoryParts
                .Where(ssp => ssp.ServerStory.ServerStoryId == dbStory.ServerStoryId)
                .OrderByDescending(ssp => ssp.PartNumber)
                .FirstOrDefaultAsync();

            if (DateTime.UtcNow - lastPart.TimeOfAddition < TimeSpan.FromSeconds(15)) return false;

            _botContext.ServerStoryParts.Add(new ServerStoryPart
            {
                ServerStory = dbStory,
                Part = line,
                PartAuthor = su,
                TimeOfAddition = DateTime.UtcNow,
                PartNumber = lastPart.PartNumber + 1
            });

            await _botContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StartNewStoryAsync(IGuildUser user, string line)
        {
            var server = await _botContext.Servers.FirstOrDefaultAsync(s => s.DiscordGuildId == (long) user.GuildId);

            var su = await _botContext.ServerUsers
                .FirstOrDefaultAsync(u => u.User.DiscordUserId == (long) user.Id &&
                                          u.Server.DiscordGuildId == (long) user.GuildId);

            var dbStory = await _botContext.ServerStories
                .Where(ss => ss.Server.DiscordGuildId == (long) user.GuildId)
                .Include(ss => ss.ServerStoryParts)
                .FirstOrDefaultAsync(ss => ss.EndTime < ss.StartTime);

            if (dbStory != null) return false;

            var serverStory = new ServerStory
            {
                Server = server,
                StartTime = DateTime.UtcNow,
                StoryInitiatedBy = su
            };
            _botContext.ServerStories.Add(serverStory);
            _botContext.ServerStoryParts.Add(new ServerStoryPart
            {
                PartNumber = 1,
                PartAuthor = su,
                Part = line,
                ServerStory = serverStory,
                TimeOfAddition = DateTime.UtcNow
            });

            await _botContext.SaveChangesAsync();
            return true;
        }
    }
}
