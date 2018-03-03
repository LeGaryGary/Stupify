using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TicTacZap;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.TicTacZapManagement
{
    internal static class Inventories
    {
        private const string InventoryExtension = ".INV";
        private static readonly string InventoriesPath;

        static Inventories()
        {
            InventoriesPath = Config.DataDirectory + @"\Inventories";
            Directory.CreateDirectory(InventoriesPath);
        }

        public static async Task<Inventory> GetInventoryAsync(int userId)
        {
            if (File.Exists(InventoriesPath + $@"\{userId + InventoryExtension}"))
            {
                var fileText = await File.ReadAllTextAsync(InventoriesPath + $@"\{userId + InventoryExtension}");
                return JsonConvert.DeserializeObject<Inventory>(fileText);
            }

            var inventory = new Inventory(5);
            await SaveInventoryAsync(userId, inventory);
            return inventory;
        }

        public static async Task SaveInventoryAsync(int userId, Inventory inventory)
        {
            var fileText = JsonConvert.SerializeObject(inventory);
            await File.WriteAllTextAsync(InventoriesPath + $@"\{userId + InventoryExtension}", fileText);
        }

        public static async Task AddToInventoryAsync(BlockType blockType, int quantity, int userId)
        {
            var inventory = await GetInventoryAsync(userId);
            inventory.AddBlocks(blockType, quantity);
            await SaveInventoryAsync(userId, inventory);
        }

        public static async Task<bool> RemoveFromInventoryAsync(BlockType blockType, int quantity, int userId)
        {
            var inventory = await GetInventoryAsync(userId);
            if (!inventory.RemoveBlocks(blockType, quantity)) return false;
            await SaveInventoryAsync(userId, inventory);
            return true;
        }

        public static async Task ResetInventory(int userId)
        {
            var inventory = await GetInventoryAsync(userId);
            inventory.Reset();
            await SaveInventoryAsync(userId, inventory);
        }
    }
}