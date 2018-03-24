using System.Threading.Tasks;
using Discord;
using Stupify.Data.Models;

namespace Stupify.Data.Repositories
{
    public interface IStoryRepository
    {
        Task<Story> RandomStoryAsync(IGuild guild, IDiscordClient client);
        Task<Story> GetCurrentStoryAsync(IGuild guild);
        Task<bool> EndCurrentStoryAsync(IGuild guild);
        Task<bool> AddToCurrentStoryAsync(IGuildUser user, string line);
        Task<bool> StartNewStoryAsync(IGuildUser user, string line);
    }
}
