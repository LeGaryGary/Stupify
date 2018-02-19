using System.Threading.Tasks;
using Discord.Commands;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.Commands.Conditions;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Commands.Modules
{
    [Debug]
    [DevOnly]
    public class Debug : StupifyModuleBase
    {
        [Command("Motherlode")]
        public async Task DebugMotherlode()
        {
            var user = await this.GetUserAsync();

            user.Balance += 1000000;

            await Db.SaveChangesAsync();
            await ReplyAsync($"You filthy cheater! Fine. I updated the balance. (balance: {user.Balance})");
        }

        [Command("InvReset")]
        public async Task DebugInvReset()
        {
            var user = this.GetUserAsync();
            await Inventories.ResetInventory(user.Id);
            await ReplyAsync("inventory reset!");
        }

        [Command("BalReset")]
        public async Task DebugBalReset()
        {
            var user = await this.GetUserAsync();

            user.Balance = 500;

            await Db.SaveChangesAsync();
            await ReplyAsync($"balance reset! (balance: {user.Balance})");
        }
    }
}
