using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Stupify.Data.Models;

namespace Stupify.Data.Repositories
{
    internal class StoryRepository : IStoryRepository
    {
        public Task<Story> RandomStoryAsync(IGuild guild, IDiscordClient client)
        {
            throw new NotImplementedException();
        }

        public Task<Story> GetCurrentStoryAsync(IGuild guild)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EndCurrentStoryAsync(IGuild guild)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddToCurrentStoryAsync(IGuildUser user, string line)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StartNewStoryAsync(IGuildUser user, string line)
        {
            throw new NotImplementedException();
        }
    }
}
