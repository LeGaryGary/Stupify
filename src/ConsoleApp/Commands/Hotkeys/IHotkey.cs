using System.Threading.Tasks;
using Discord.Commands;

namespace StupifyConsoleApp.Commands.Hotkeys
{
    internal interface IHotkey
    {
        char Key { get; }
        Task ExecuteAsync(ICommandContext context);
    }
}
