using System.Threading.Tasks;
using Discord;
using TicTacZap;
using TicTacZap.Blocks;

namespace Stupify.Data.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory> GetInventoryAsync(IUser user);
        Task SaveInventoryAsync(IUser user, Inventory inventory);
        Task AddToInventoryAsync(BlockType blockType, int quantity, IUser user);
        Task<bool> RemoveFromInventoryAsync(BlockType blockType, int quantity, IUser user);
        Task ResetInventory(IUser user);
    }
}
