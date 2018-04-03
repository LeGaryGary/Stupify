using System;
using System.Threading.Tasks;
using Discord.Commands;
using Stupify.Data.Repositories;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class Inventory : ModuleBase<CommandContext>
    {
        private readonly TicTacZapController _tacZapController;
        private readonly IUserRepository _userRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public Inventory(TicTacZapController tacZapController, IUserRepository userRepository, IInventoryRepository inventoryRepository)
        {
            _tacZapController = tacZapController;
            _userRepository = userRepository;
            _inventoryRepository = inventoryRepository;
        }

        [Command("Balance")]
        public async Task ShowBalance()
        {
            await ReplyAsync($"Your balance is: {await _userRepository.BalanceAsync(Context.User)}");
        }

        [Command("Inventory")]
        public async Task ShowInventory()
        {
            var inventory = await _inventoryRepository.GetInventoryAsync(Context.User);
            var message = inventory.TextRender();
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
            
            if (!await _userRepository.UserToBankTransferAsync(Context.User, total.Value))
            {
                await ReplyAsync(Responses.NotEnoughUnits(total.Value));
                return;
            }

            await _inventoryRepository.AddToInventoryAsync(block, quantity, Context.User);
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
            
            if (!await _userRepository.BankToUserTransferAsync(Context.User, total.Value))
            {
                await ReplyAsync(Responses.NotEnoughUnits(total.Value));
                return;
            }

            if (!await _inventoryRepository.RemoveFromInventoryAsync(block, quantity, Context.User))
            {
                await _userRepository.UserToBankTransferAsync(Context.User, total.Value);
                await ReplyAsync("You don't have the required blocks...");
            }
            await ShowInventory();
        }
    }
}