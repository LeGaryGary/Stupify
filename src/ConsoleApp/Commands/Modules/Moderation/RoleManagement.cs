using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using StupifyConsoleApp.Commands.Conditions;

namespace StupifyConsoleApp.Commands.Modules.Moderation
{
    [Group("Role")]
    public class RoleManagement : ModuleBase<CommandContext>
    {
        [Command("Add")]
        [Moderator]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddRoleCommand(IRole role, IGuildUser guildUser)
        {
            if (!guildUser.RoleIds.Contains(role.Id))
            {
                await guildUser.AddRoleAsync(role);
                var embed = new EmbedBuilder
                {
                    ThumbnailUrl = guildUser.GetAvatarUrl(),
                    Title = "The role has been added!",
                    Color = Color.Purple
                };
                embed.AddField("Role", $"{role.Name} ({role.Id})");
                embed.AddField("User", guildUser.Nickname == null ? $"{guildUser.Username}#{guildUser.Discriminator}" : $"{guildUser.Nickname} ({guildUser.Username}#{guildUser.Discriminator})");
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    ThumbnailUrl = guildUser.GetAvatarUrl(),
                    Title = "The user already has this role!",
                    Color = Color.Red
                };
                embed.AddField("Role", $"{role.Name} ({role.Id})");
                embed.AddField("User", guildUser.Nickname == null ? $"{guildUser.Username}#{guildUser.Discriminator}" : $"{guildUser.Nickname} ({guildUser.Username}#{guildUser.Discriminator})");
                await ReplyAsync(embed: embed.Build());
            }
        }

        [Command("Remove")]
        [Moderator]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRoleCommand(IRole role, IGuildUser guildUser)
        {
            if (guildUser.RoleIds.Contains(role.Id))
            {
                await guildUser.RemoveRoleAsync(role);
                var embed = new EmbedBuilder
                {
                    ThumbnailUrl = guildUser.GetAvatarUrl(),
                    Title = "The role has been removed!",
                    Color = Color.Purple
                };
                embed.AddField("Role", $"{role.Name} ({role.Id})");
                embed.AddField("User", guildUser.Nickname == null ? $"{guildUser.Username}#{guildUser.Discriminator}" : $"{guildUser.Nickname} ({guildUser.Username}#{guildUser.Discriminator})");
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    ThumbnailUrl = guildUser.GetAvatarUrl(),
                    Title = "The user doesnt have this role!",
                    Color = Color.Red
                };
                embed.AddField("Role", $"{role.Name} ({role.Id})");
                embed.AddField("User", guildUser.Nickname == null ? $"{guildUser.Username}#{guildUser.Discriminator}" : $"{guildUser.Nickname} ({guildUser.Username}#{guildUser.Discriminator})");
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}
