using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace StupifyConsoleApp.Commands.Conditions
{
    public class DebugAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(Config.Debug ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Error: This is a debug command"));
        }
    }
}