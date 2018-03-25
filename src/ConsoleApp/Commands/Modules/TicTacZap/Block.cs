using System;
using System.Threading.Tasks;
using Discord.Commands;
using Stupify.Data;
using Stupify.Data.Repositories;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class Block : ModuleBase<CommandContext>
    {
        private readonly TicTacZapController _tacZapController;
        private readonly GameState _gameState;
        private readonly IUserRepository _userRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ISegmentRepository _segmentRepository;

        public Block(TicTacZapController tacZapController, GameState gameState, IUserRepository userRepository, IInventoryRepository inventoryRepository, ISegmentRepository segmentRepository)
        {
            _tacZapController = tacZapController;
            _gameState = gameState;
            _userRepository = userRepository;
            _inventoryRepository = inventoryRepository;
            _segmentRepository = segmentRepository;
        }

        [Command("BlockInfo")]
        public async Task BlockInfoCommand(int x, int y)
        {
            var segmentSelectionId = _gameState.GetUserSegmentSelection(await _userRepository.GetUserId(Context.User));
            if (!segmentSelectionId.HasValue)
            {
                await ReplyAsync(Responses.SelectSegmentMessage);
                return;
            }

            var text = await _tacZapController.RenderBlockInfoAsync(segmentSelectionId.Value, x-1, y-1);
            if (string.IsNullOrEmpty(text))
            {
                await ReplyAsync(Responses.NoSuchBlock);
                return;
            }

            await ReplyAsync($"```{text}```");
        }

        [Command("AddBlock")]
        public async Task AddBlockCommand(int x, int y, string type)
        {
            var segmentSelectionId = _gameState.GetUserSegmentSelection(await _userRepository.GetUserId(Context.User));
            if (segmentSelectionId.HasValue)
            {
                await AddBlockCommand(segmentSelectionId.Value, x, y, type);
                return;
            }

            await ReplyAsync(Responses.SelectSegmentMessage);
        }

        private async Task AddBlockCommand(int segmentId, int x, int y, string type)
        {
            var blockType = Enum.Parse<BlockType>(type, true);
            if (await _inventoryRepository.RemoveFromInventoryAsync(blockType, 1, Context.User))
            {
                if (!await _segmentRepository.AddBlockAsync(segmentId, x - 1, y - 1, blockType))
                {
                    await _inventoryRepository.AddToInventoryAsync(blockType, 1, Context.User);
                }
                await _tacZapController.ShowSegmentAsync(Context, segmentId);
                return;
            }

            await ReplyAsync(Responses.ShopAdvisoryMessage);
        }

        [Command("RemoveBlock")]
        public async Task RemoveBlockCommand(int x, int y)
        {
            var segmentSelectionId = _gameState.GetUserSegmentSelection(await _userRepository.GetUserId(Context.User));
            if (segmentSelectionId.HasValue)
            {
                await RemoveBlockCommand(segmentSelectionId.Value, x, y);
                return;
            }

            await ReplyAsync(Responses.SelectSegmentMessage);
        }

        private async Task RemoveBlockCommand(int segmentId, int x, int y)
        {
            var blockType = await _segmentRepository.DeleteBlockAsync(segmentId, x - 1, y - 1);

            if (blockType != null)
            {
                await _inventoryRepository.AddToInventoryAsync(blockType.Value, 1, Context.User);
            }
            await _tacZapController.ShowSegmentAsync(Context, segmentId);
        }
    }
}