using Discord.Commands;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Commands
{
    public abstract class StupifyModuleBase : ModuleBase<CommandContext>
    {
        public readonly BotContext Db;

        protected StupifyModuleBase(BotContext db)
        {
            Db = db;
        }
    }
}