using System;
using System.Threading.Tasks;
using Discord.Commands;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;
using TicTacZap.Segment.Blocks;

namespace StupifyConsoleApp.Commands.TicTacZap
{
    public class InventoryModule:ModuleBase<SocketCommandContext>
    {
        private BotContext Db { get; } = new BotContext();

        [Command("balance")]
        public async Task ShowBalance()
        {
            var balance = await Balance();
            await ReplyAsync($"Your balance is: {balance}");
        }

        private async Task<decimal> Balance()
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);
            return user.Balance;
        }

        [Command("inventory")]
        public async Task ShowInventory()
        {
            var userId = (await CommonFunctions.GetUserAsync(Db, Context)).UserId;
            var message = await TicTacZapController.RenderInventory(userId);
            if (message == string.Empty)
            {
                await ReplyAsync("Your inventory is empty");
                return;
            }
            await ReplyAsync(message);
        }

        [Command("shop")]
        public async Task ShowShopInventory()
        {
            var message = TicTacZapController.Shop.TextRender();
            await ReplyAsync(message);
        }

        [Command("buy")]
        public async Task BuyFromShop(string blockString, int quantity)
        {
            var block = Enum.Parse<BlockType>(blockString);
            var total = TicTacZapController.Shop.GetTotal(block, quantity);
            if (await RemoveBalanceAsync(total))
            {
                await Inventories.AddToInventoryAsync(block, quantity, (await CommonFunctions.GetUserAsync(Db, Context)).UserId);
                await ShowInventory();
                return;
            }

            await ReplyAsync(CommonFunctions.NotEnoughUnits(total));
        }

        private async Task<bool> RemoveBalanceAsync(decimal units)
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);
            if (units > user.Balance) return false;
            user.Balance -= units;
            await Db.SaveChangesAsync();
            return true;
        }
    }
}
