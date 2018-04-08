using System.Threading.Tasks;
using Discord.Commands;
using Stupify.Data.Repositories;
using StupifyConsoleApp.Commands.Conditions;

namespace StupifyConsoleApp.Commands.Modules
{
    [Debug]
    [DevOnly]
    public class Debug : ModuleBase<CommandContext>
    {
        private readonly IUserRepository _userRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public Debug(IUserRepository userRepository, IInventoryRepository inventoryRepository)
        {
            _userRepository = userRepository;
            _inventoryRepository = inventoryRepository;
        }

        [Command("Motherlode")]
        public async Task DebugMotherlode()
        {
            await _userRepository.BankToUserTransferAsync(Context.User, 1000000);
            await ReplyAsync($"You filthy cheater! Fine. I updated the balance. (balance: {await _userRepository.BalanceAsync(Context.User)})");
        }

        [Command("InvReset")]
        public async Task DebugInvReset()
        {
            await _inventoryRepository.ResetInventory(Context.User);
            await ReplyAsync("inventory reset!");
        }

        [Command("BalReset")]
        public async Task DebugBalReset()
        {
            var amount = await _userRepository.BalanceAsync(Context.User) - 500;

            await _userRepository.UserToBankTransferAsync(Context.User, amount);
            await ReplyAsync($"balance reset! (balance: {_userRepository.BalanceAsync(Context.User)})");
        }
    }
}