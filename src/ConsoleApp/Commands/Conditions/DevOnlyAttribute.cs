using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace StupifyConsoleApp.Commands.Conditions
{
    public class DevOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            if (context.User is IGuildUser user && user.RoleIds.Contains(Config.DeveloperRole))
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(
                PreconditionResult.FromError("Trying to be sneaky eh? sorry only devs can do that :^)"));
        }
    }
}