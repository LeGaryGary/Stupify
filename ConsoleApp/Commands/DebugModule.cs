using Discord.Commands;
using System.Threading.Tasks;

using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;

namespace StupifyConsoleApp.Commands
{
    public class DebugModule : ModuleBase<SocketCommandContext>
    {
        private BotContext Db { get; } = new BotContext();

        [Command("motherlode")]
        public async Task DebugMotherlode()
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);

            user.Balance += 1000000;

            await Db.SaveChangesAsync();
            await ReplyAsync($"You filthy cheater! Fine. I updated the balance. (balance: {user.Balance})");
        }

        [Command("invreset")]
        public async Task DebugInvReset()
        {
            var user = CommonFunctions.GetUserAsync(Db, Context);
            await TicTacZapController.ResetInventory(user.Id);
            await ReplyAsync("inventory reset!");
        }

        [Command("balreset")]
        public async Task DebugBalReset()
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);

            user.Balance = 500;

            await Db.SaveChangesAsync();
            await ReplyAsync($"balance reset! (balance: {user.Balance})");
        }
    }
}
