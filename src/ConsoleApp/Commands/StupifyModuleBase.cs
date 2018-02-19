using Discord.Commands;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Commands
{
    public abstract class StupifyModuleBase : ModuleBase<SocketCommandContext>
    {
        public BotContext Db { get; } = new BotContext();
    }
}