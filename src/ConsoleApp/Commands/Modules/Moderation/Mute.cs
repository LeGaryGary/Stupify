using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Commands.Modules.Moderation
{
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public class Mute : StupifyModuleBase
    {
        public Mute(BotContext db) : base(db)
        {
        }

        [Command("mute")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task MuteAsync(string userTag)
        {
            var serverUser = await Db.GetServerUserAsync(ulong.Parse(Regex.Replace(userTag, "[^0-9]", "")),
                Context.Guild.Id);
            if (serverUser == null)
            {
                await ReplyAsync("Thats not right!");
                return;
            }

            serverUser.Muted = true;
            await Db.SaveChangesAsync();
            await ReplyAsync("This muggle has been silenced!");
        }

        [Command("unmute")]
        public async Task UnMuteAsync([Remainder] string cmdStr)
        {
            var serverUser = await Db.GetServerUserAsync(ulong.Parse(Regex.Replace(cmdStr, "[^0-9]", "")), Context.Guild.Id);
            serverUser.Muted = false;
            var saveTask = Db.SaveChangesAsync();
            await ReplyAsync("This muggle has been forgiven, for now...");
            await saveTask;
        }
    }
}