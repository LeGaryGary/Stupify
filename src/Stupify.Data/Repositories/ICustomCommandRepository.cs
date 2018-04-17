using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Stupify.Data.Repositories
{
    public interface ICustomCommandRepository
    {
        Task<string> GetCommandAsync(IGuildUser user, string commandTag);
        Task AddCommandAsync(IGuildUser user, string commandTag, string command);
        Task<string[]> ListServerCommandsAsync(IGuildUser user);
        Task<string[]> ListServerUserCommandsAsync(IGuildUser user);
        Task ExecuteAsync(ICommandContext context, int argPos);
        Task<string> EvaluateAsync(string command);
        Task<bool> IsCreatorAsync(IGuildUser user, string commandTag);
        Task DeleteAsync(IGuildUser user, string commandTag);
    }
}
