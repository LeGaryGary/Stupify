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
        public async Task ShowBalanceAsync()
        {
            await ReplyAsync($"Your balance is: {await _userRepository.BalanceAsync(Context.User).ConfigureAwait(false)}").ConfigureAwait(false);
        }

        [Command("Inventory")]
        public async Task ShowInventoryAsync()
        {
            var inventory = await _inventoryRepository.GetInventoryAsync(Context.User).ConfigureAwait(false);
            var message = inventory.TextRender();
            if (message == string.Empty)
            {
                await ReplyAsync("Your inventory is empty").ConfigureAwait(false);
                return;
            }

            await ReplyAsync("Inventory:" + Environment.NewLine + message).ConfigureAwait(false);
        }

        [Command("Shop")]
        public Task ShowShopInventoryAsync()
        {
            var message = "Buy: 105%\r\nSell: 95%\r\n";
            message += _tacZapController.Shop.TextRender();
            return ReplyAsync(message);
        }

        [Command("Buy")]
        public async Task BuyFromShopAsync(BlockType block, int quantity)
        {
            var total = _tacZapController.Shop.GetBuyTotal(block, quantity);
            if (!total.HasValue)
            {
                await ReplyAsync("The shop doesn't sell this type of block.").ConfigureAwait(false);
                return;
            }
            
            if (!await _userRepository.UserToBankTransferAsync(Context.User, total.Value).ConfigureAwait(false))
            {
                await ReplyAsync(Responses.NotEnoughUnits(total.Value)).ConfigureAwait(false);
                return;
            }

            await _inventoryRepository.AddToInventoryAsync(block, quantity, Context.User).ConfigureAwait(false);
            await ShowInventoryAsync().ConfigureAwait(false);
        }

        [Command("Sell")]
        public async Task SellToShopAsync(BlockType block, int quantity)
        {
            var total = _tacZapController.Shop.GetSellTotal(block, quantity);
            if (!total.HasValue)
            {
                await ReplyAsync("The shop doesn't buy this type of block.").ConfigureAwait(false);
                return;
            }
            
            if (!await _userRepository.BankToUserTransferAsync(Context.User, total.Value).ConfigureAwait(false))
            {
                await ReplyAsync(Responses.NotEnoughUnits(total.Value)).ConfigureAwait(false);
                return;
            }

            if (!await _inventoryRepository.RemoveFromInventoryAsync(block, quantity, Context.User).ConfigureAwait(false))
            {
                await _userRepository.UserToBankTransferAsync(Context.User, total.Value).ConfigureAwait(false);
                await ReplyAsync("You don't have the required blocks...").ConfigureAwait(false);
            }
            await ShowInventoryAsync().ConfigureAwait(false);
        }
    }
}