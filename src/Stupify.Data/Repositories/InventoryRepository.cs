using System.Threading.Tasks;
using Discord;
using Stupify.Data.FileSystem;
using TicTacZap;
using TicTacZap.Blocks;

namespace Stupify.Data.Repositories
{
    internal class InventoryRepository : IInventoryRepository
    {
        private readonly IUserRepository _userRepository;
        private readonly Inventories _inventories;

        public InventoryRepository(IUserRepository userRepository, Inventories inventories)
        {
            _userRepository = userRepository;
            _inventories = inventories;
        }

        public async Task<Inventory> GetInventoryAsync(IUser user)
        {
            return await _inventories.GetInventoryAsync(
                await _userRepository.GetUserIdAsync(user).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task SaveInventoryAsync(IUser user, Inventory inventory)
        {
            await _inventories.SaveInventoryAsync(
                await _userRepository.GetUserIdAsync(user).ConfigureAwait(false),
                inventory).ConfigureAwait(false);
        }

        public async Task AddToInventoryAsync(BlockType blockType, int quantity, IUser user)
        {
            await _inventories.AddToInventoryAsync(
                blockType,
                quantity,
                await _userRepository.GetUserIdAsync(user).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task<bool> RemoveFromInventoryAsync(BlockType blockType, int quantity, IUser user)
        {
            return await _inventories.RemoveFromInventoryAsync(
                blockType,
                quantity,
                await _userRepository.GetUserIdAsync(user).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task ResetInventoryAsync(IUser user)
        {
            await _inventories.ResetInventoryAsync(
                await _userRepository.GetUserIdAsync(user).ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}
