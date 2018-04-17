using System;
using System.Threading.Tasks;
using Discord.Commands;
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
        public async Task BlockInfoCommandAsync(int x, int y)
        {
            var segmentSelectionId = _gameState.GetUserSegmentSelection(await _userRepository.GetUserIdAsync(Context.User).ConfigureAwait(false));
            if (!segmentSelectionId.HasValue)
            {
                await ReplyAsync(Responses.SelectSegmentMessage).ConfigureAwait(false);
                return;
            }

            var text = await _tacZapController.RenderBlockInfoAsync(segmentSelectionId.Value, x-1, y-1).ConfigureAwait(false);
            if (string.IsNullOrEmpty(text))
            {
                await ReplyAsync(Responses.NoSuchBlock).ConfigureAwait(false);
                return;
            }

            await ReplyAsync($"```{text}```").ConfigureAwait(false);
        }

        [Command("AddBlock")]
        public async Task AddBlockCommandAsync(int x, int y, string type)
        {
            var segmentSelectionId = _gameState.GetUserSegmentSelection(await _userRepository.GetUserIdAsync(Context.User).ConfigureAwait(false));
            if (segmentSelectionId.HasValue)
            {
                await AddBlockCommandAsync(segmentSelectionId.Value, x, y, type).ConfigureAwait(false);
                return;
            }

            await ReplyAsync(Responses.SelectSegmentMessage).ConfigureAwait(false);
        }

        [Command("RemoveBlock")]
        public async Task RemoveBlockCommandAsync(int x, int y)
        {
            var segmentSelectionId = _gameState.GetUserSegmentSelection(await _userRepository.GetUserIdAsync(Context.User).ConfigureAwait(false));
            if (segmentSelectionId.HasValue)
            {
                await RemoveBlockCommandAsync(segmentSelectionId.Value, x, y).ConfigureAwait(false);
                return;
            }

            await ReplyAsync(Responses.SelectSegmentMessage).ConfigureAwait(false);
        }

        private async Task AddBlockCommandAsync(int segmentId, int x, int y, string type)
        {
            var blockType = Enum.Parse<BlockType>(type, true);
            if (await _inventoryRepository.RemoveFromInventoryAsync(blockType, 1, Context.User).ConfigureAwait(false))
            {
                if (!await _segmentRepository.AddBlockAsync(segmentId, x - 1, y - 1, blockType).ConfigureAwait(false))
                {
                    await _inventoryRepository.AddToInventoryAsync(blockType, 1, Context.User).ConfigureAwait(false);
                }
                await _tacZapController.ShowSegmentAsync(Context, segmentId).ConfigureAwait(false);
                return;
            }

            await ReplyAsync(Responses.ShopAdvisoryMessage).ConfigureAwait(false);
        }

        private async Task RemoveBlockCommandAsync(int segmentId, int x, int y)
        {
            var blockType = await _segmentRepository.DeleteBlockAsync(segmentId, x - 1, y - 1).ConfigureAwait(false);

            if (blockType != null)
            {
                await _inventoryRepository.AddToInventoryAsync(blockType.Value, 1, Context.User).ConfigureAwait(false);
            }
            await _tacZapController.ShowSegmentAsync(Context, segmentId).ConfigureAwait(false);
        }
    }
}