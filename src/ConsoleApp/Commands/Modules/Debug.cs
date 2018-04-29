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
        public async Task DebugMotherlodeAsync()
        {
            await _userRepository.BankToUserTransferAsync(Context.User, 1000000).ConfigureAwait(false);
            await ReplyAsync($"You filthy cheater! Fine. I updated the balance. (balance: {await _userRepository.BalanceAsync(Context.User).ConfigureAwait(false)})").ConfigureAwait(false);
        }

        [Command("InvReset")]
        public async Task DebugInvResetAsync()
        {
            await _inventoryRepository.ResetInventoryAsync(Context.User).ConfigureAwait(false);
            await ReplyAsync("inventory reset!").ConfigureAwait(false);
        }

        [Command("BalReset")]
        public async Task DebugBalResetAsync()
        {
            var amount = await _userRepository.BalanceAsync(Context.User).ConfigureAwait(false) - 500;

            await _userRepository.UserToBankTransferAsync(Context.User, amount).ConfigureAwait(false);
            await ReplyAsync($"balance reset! (balance: {_userRepository.BalanceAsync(Context.User)})").ConfigureAwait(false);
        }
    }
}