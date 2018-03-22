using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Stupify.Data.Models;

namespace Stupify.Data
{
    public interface IStoryRepository
    {
        Task<Story> RandomStoryAsync(IGuild guild);
        Task<Story> GetCurrentStoryAsync(IGuild guild);
        Task<bool> EndCurrentStoryAsync(IGuild guild);
        Task<bool> AddToCurrentStoryAsync(IGuildUser user, string line);
        Task<bool> StartNewStoryAsync(IGuildUser user, string line);
    }
}
