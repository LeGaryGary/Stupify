using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Commands.Moderation
{
    public class MuteModule : ModuleBase<SocketCommandContext>
    {
        [Command("mute"), RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task MuteAsync([Remainder] string cmdStr)
        {
            var cmdArray = cmdStr.Split(" ");
            if (cmdArray.Length != 2)
            {
                await ReplyAsync("Too many bits and bobs, please use that spell like this: " +
                                 "" + Config.CommandPrefix + "mute TagAMuggle 00:10:00");
                return;
            }

            using (var db = new BotContext())
            {
                var serverUser = await db.GetServerUserAsync(ulong.Parse(Regex.Replace(cmdArray[0], "[^0-9]", "")), Context.Guild.Id);
                if (serverUser == null)
                {
                    await ReplyAsync("Thats not right!");
                    return;
                }
                serverUser.Muted = true;
                var saveTask = db.SaveChangesAsync();
                await ReplyAsync("This muggle has been silenced!");
                await saveTask;
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
