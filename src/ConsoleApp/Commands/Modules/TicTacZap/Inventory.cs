using System;
using System.Threading.Tasks;
using Discord.Commands;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class Inventory : StupifyModuleBase
    {
        private readonly TicTacZapController _tacZapController;

        public Inventory(BotContext db, TicTacZapController tacZapController) : base(db)
        {
            _tacZapController = tacZapController;
        }

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
            var message = await _tacZapController.RenderInventory(userId);
            if (message == string.Empty)
            {
                await ReplyAsync("Your inventory is empty");
                return;
            }

            await ReplyAsync("Inventory:" + Environment.NewLine + message);
        }

        [Command("Shop")]
        public async Task ShowShopInventory()
        {
            var message = "Buy: 105%\r\nSell: 95%\r\n";
            message += _tacZapController.Shop.TextRender();
            await ReplyAsync(message);
        }

        [Command("Buy")]
        public async Task BuyFromShop(BlockType block, int quantity)
        {
            var total = _tacZapController.Shop.GetBuyTotal(block, quantity);
            if (!total.HasValue)
            {
                await ReplyAsync("The shop doesn't sell this type of block.");
                return;
            }

            if (!_tacZapController.MakeTransaction(
                await this.GetUserAsync(),
                await _tacZapController.GetBankAsync(),
                total.Value))
            {
                await ReplyAsync(Responses.NotEnoughUnits(total.Value));
                return;
            }

            await Db.SaveChangesAsync();
            await Inventories.AddToInventoryAsync(block, quantity, (await this.GetUserAsync()).UserId);
            await ShowInventory();
        }

        [Command("Sell")]
        public async Task SellToShop(BlockType block, int quantity)
        {
            var total = _tacZapController.Shop.GetSellTotal(block, quantity);
            if (!total.HasValue)
            {
                await ReplyAsync("The shop doesn't buy this type of block.");
                return;
            }

            if (!_tacZapController.MakeTransaction(
                await _tacZapController.GetBankAsync(),
                await this.GetUserAsync(),
                total.Value))
            {
                await ReplyAsync(Responses.NotEnoughUnits(total.Value));
                return;
            }

            if (await Inventories.RemoveFromInventoryAsync(block, quantity, (await this.GetUserAsync()).UserId)) await Db.SaveChangesAsync();
            else
            {
                await ReplyAsync("You don't have the required blocks...");
            }
            await ShowInventory();
        }
    }
}