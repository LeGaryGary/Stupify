using System;
using System.Threading.Tasks;
using Discord.Commands;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap.Segment.Blocks;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class Inventory : StupifyModuleBase
    {
        [Command("Balance")]
        public async Task ShowBalance()
        {
            var balance = await Balance();
            await ReplyAsync($"Your balance is: {balance}");
        }

        private async Task<decimal> Balance()
        {
            var user = await this.GetUserAsync();
            return user.Balance;
        }

        [Command("Inventory")]
        public async Task ShowInventory()
        {
            var userId = (await this.GetUserAsync()).UserId;
            var message = await TicTacZapController.RenderInventory(userId);
            if (message == string.Empty)
            {
                await ReplyAsync("Your inventory is empty");
                return;
            }

            await ReplyAsync(message);
        }

        [Command("Shop")]
        public async Task ShowShopInventory()
        {
            var message = TicTacZapController.Shop.TextRender();
            await ReplyAsync(message);
        }

        [Command("Buy")]
        public async Task BuyFromShop(string blockString, int quantity)
        {
            var block = Enum.Parse<BlockType>(blockString);
            var total = TicTacZapController.Shop.GetTotal(block, quantity);
            if (await RemoveBalanceAsync(total))
            {
                await Inventories.AddToInventoryAsync(block, quantity, (await this.GetUserAsync()).UserId);
                await ShowInventory();
                return;
            }

            await ReplyAsync(Responses.NotEnoughUnits(total));
        }

        private async Task<bool> RemoveBalanceAsync(decimal units)
        {
            var user = await this.GetUserAsync();
            if (units > user.Balance) return false;
            user.Balance -= units;
            await Db.SaveChangesAsync();
            return true;
        }
    }
}