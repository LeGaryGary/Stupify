using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Commands.Moderation
{
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public class MuteModule : ModuleBase<SocketCommandContext>
    {
        [Command("mute"), RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task MuteAsync(string userTag)
        {
            using (var db = new BotContext())
            {
                var serverUser = await db.GetServerUserAsync(ulong.Parse(Regex.Replace(userTag, "[^0-9]", "")), Context.Guild.Id);
                if (serverUser == null)
                {
                    await ReplyAsync("Thats not right!");
                    return;
                }
                serverUser.Muted = true;
                await db.SaveChangesAsync();
                await ReplyAsync("This muggle has been silenced!");
            }
        }

        [Command("unmute")]
        public async Task UnMuteAsync([Remainder] string cmdStr)
        {
            using (var db = new BotContext())
            {
                var serverUser = await db.GetServerUserAsync(ulong.Parse(Regex.Replace(cmdStr, "[^0-9]", "")), Context.Guild.Id);
                serverUser.Muted = false;
                var saveTask = db.SaveChangesAsync();
                await ReplyAsync("This muggle has been forgiven, for now...");
                await saveTask;
            }
        }
    }
}
