using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Stupify.Data.Repositories;

namespace StupifyConsoleApp.Commands.Conditions
{
    public class ModeratorAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            if (!(context.User is IGuildUser user) || user.GuildPermissions.Administrator) return PreconditionResult.FromSuccess();

            var settingsRepo = services.GetService<ISettingsRepository>();
            var settings = await settingsRepo.GetServerSettingsAsync(context.Guild.Id).ConfigureAwait(false);

            if (settings.ModeratorRoleId == null) return PreconditionResult.FromError("No moderator role setup! http://stupifybot.com/GuildSettings");

            var roleDisplayString = user.Guild.GetRole(settings.ModeratorRoleId.Value).Name ?? settings.ModeratorRoleId.Value.ToString();

            return user.RoleIds.Contains(settings.ModeratorRoleId.Value) 
                ? PreconditionResult.FromSuccess() 
                : PreconditionResult.FromError($"You don't have the role {roleDisplayString}!");
        }
    }
}