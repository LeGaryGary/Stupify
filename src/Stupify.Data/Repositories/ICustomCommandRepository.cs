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
        Task ExecuteAsync(ICommandContext context, int argPos);
    }
}
