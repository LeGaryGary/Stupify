using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Stupify.Data.Repositories;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class Segments : ModuleBase<CommandContext>
    {
        private readonly ISegmentRepository _segmentRepository;
        private readonly IUniverseRepository _universeRepository;

        public Segments(ISegmentRepository segmentRepository, IUniverseRepository universeRepository)
        {
            _segmentRepository = segmentRepository;
            _universeRepository = universeRepository;
        }

        [Command("Segments")]
        public async Task ListSegmentsAsync()
        {
            var segments = await _segmentRepository.GetSegmentIds(Context.User).ConfigureAwait(false);
            var renderSegmentList = await RenderSegmentListAsync(segments).ConfigureAwait(false);
            if (renderSegmentList == string.Empty)
                await ReplyAsync($"You don't have any segments, buy your first one: `{Config.CommandPrefix} Segment Buy`").ConfigureAwait(false);
            else
                await ReplyAsync(renderSegmentList + Environment.NewLine + $"Use `{Config.CommandPrefix} Segment [segmentId]` to select and show a segment!").ConfigureAwait(false);
        }

        private async Task<string> RenderSegmentListAsync(IEnumerable<int> segmentIds)
        {
            var render = string.Empty;

            foreach (var segmentId in segmentIds)
            {
                var location = await _universeRepository.FindAsync(segmentId).ConfigureAwait(false);
                var x = "~";
                var y = "~";
                if (location.HasValue)
                {
                    x = location.Value.x.ToString();
                    y = location.Value.y.ToString();
                }
                
                render += $"Segment Id: {segmentId} Location: {x}, {y}" + Environment.NewLine;
            }

            return render;
        }

        private static decimal SegmentPrice(int segmentCount)
        {
            return segmentCount * 100;
        }

        [Group("Segment")]
        public class SegmentModule : ModuleBase<CommandContext>
        {
            private readonly TicTacZapController _tacZapController;
            private readonly SegmentEditReactionHandler _editReactionHandler;
            private readonly GameState _gameState;
            private readonly IUserRepository _userRepository;
            private readonly ISegmentRepository _segmentRepository;
            private readonly IInventoryRepository _inventoryRepository;
            private readonly IUniverseRepository _universeRepository;
            
            public SegmentModule(TicTacZapController tacZapController, SegmentEditReactionHandler editReactionHandler, GameState gameState, IUserRepository userRepository, ISegmentRepository segmentRepository, IInventoryRepository inventoryRepository, IUniverseRepository universeRepository)
            {
                _tacZapController = tacZapController;
                _editReactionHandler = editReactionHandler;
                _gameState = gameState;
                _userRepository = userRepository;
                _segmentRepository = segmentRepository;
                _inventoryRepository = inventoryRepository;
                _universeRepository = universeRepository;
            }

            [Command]
            public async Task SegmentAsync()
            {
                var segmentId = await GetSegmentSelectionAsync().ConfigureAwait(false);
                if (segmentId.HasValue)
                {
                    await _tacZapController.ShowSegmentAsync(Context, segmentId.Value).ConfigureAwait(false);
                }
            }

            [Command]
            public async Task SegmentAsync(int segmentId)
            {
                if (await _segmentRepository.UserHasSegmentAsync(Context.User, segmentId).ConfigureAwait(false))
                {
                    await _tacZapController.ShowSegmentAsync(Context, segmentId).ConfigureAwait(false);
                    return;
                }

                await ReplyAsync(Responses.SegmentOwnershipProblem).ConfigureAwait(false);
            }

            [Command]
            public async Task SegmentAsync(int segmentId, Overlay overlayType)
            {
                if (await _segmentRepository.UserHasSegmentAsync(Context.User, segmentId).ConfigureAwait(false))
                {
                    await _tacZapController.ShowSegmentAsync(Context, segmentId, overlayType).ConfigureAwait(false);
                    return;
                }

                await ReplyAsync(Responses.SegmentOwnershipProblem).ConfigureAwait(false);
            }

            [Command("Buy")]
            public async Task BuySegmentAsync()
            {
                var price = SegmentPrice(await _segmentRepository.SegmentCountAsync(Context.User).ConfigureAwait(false));
                if (!await _userRepository.UserToBankTransferAsync(Context.User, price).ConfigureAwait(false))
                {
                    await ReplyAsync(Responses.NotEnoughUnits(price)).ConfigureAwait(false);
                    return;
                }

                var segment = await _segmentRepository.NewSegmentAsync(Context.User).ConfigureAwait(false);

                await ReplyAsync($"You have purchased a segment!\r\nId: {segment.segmentId}\r\nCoordinates: {segment.coords.Value.x+1}, {segment.coords.Value.y+1}")
                    .ConfigureAwait(false);
            }

            [Command("Reset")]
            public async Task ResetSegmentAsync()
            {
                var segmentId = await GetSegmentSelectionAsync().ConfigureAwait(false);
                if (segmentId.HasValue)
                {
                    var blocks = await _segmentRepository.ResetSegmentAsync(segmentId.Value).ConfigureAwait(false);

                    foreach (var type in blocks)
                        if (type.Value > 0)
                            await _inventoryRepository.AddToInventoryAsync(type.Key, type.Value, Context.User).ConfigureAwait(false);
                
                    await ReplyAsync($"segment {segmentId} was reset!").ConfigureAwait(false);
                }
            }

            [Command("Edit")]
            public async Task EditSegmentCommandAsync(int segmentId)
            {
                if (!await _segmentRepository.UserHasSegmentAsync(Context.User, segmentId).ConfigureAwait(false))
                {
                    await ReplyAsync(Responses.SegmentOwnershipProblem).ConfigureAwait(false);
                    return;
                }

                var msg = await ReplyAsync($"```Hang on...```").ConfigureAwait(false);
                var dbUserId = await _userRepository.GetUserId(Context.User).ConfigureAwait(false);
                await _editReactionHandler.NewOwner(msg, segmentId, Context.User.Id, dbUserId).ConfigureAwait(false);
            }

            [Command("Delete")]
            public async Task DeleteSegmentCommandAsync()
            {
                var segmentId = await GetSegmentSelectionAsync().ConfigureAwait(false);
                if (segmentId.HasValue)
                {
                    var locationNullable = await _segmentRepository.DeleteSegmentAsync(segmentId.Value).ConfigureAwait(false);
                    if (locationNullable.HasValue)
                    {
                        await ReplyAsync($"It's gone...\r\nId: {segmentId}\r\nCoordinates: {locationNullable.Value.x + 1}, {locationNullable.Value.y + 1}").ConfigureAwait(false);
                        return;
                    }

                    await ReplyAsync("Something went wrong, we couldn't find your segment in the universe, how odd").ConfigureAwait(false);
                }
            }

            [Command("Attack")]
            public async Task AttackSegmentCommandAsync(Direction direction)
            {
                var segmentId = await GetSegmentSelectionAsync().ConfigureAwait(false);
                if (segmentId.HasValue)
                {
                    if (!await _tacZapController.SegmentReadyForCombatAsync(segmentId.Value).ConfigureAwait(false))
                    {
                        await ReplyAsync("This segment isn't ready for combat (needs to have offensive blocks and not already be in combat)").ConfigureAwait(false);
                        return;
                    }

                    var defendingSegment = await _universeRepository.GetAdjacentSegmentInDirectionAsync(segmentId.Value, direction).ConfigureAwait(false);
                    if (defendingSegment.HasValue)
                    {
                        var opposite = direction.Opposite();
                        await ReplyAsync("Attacker:").ConfigureAwait(false);
                        var attackMessage = await ReplyAsync("```Loading...```").ConfigureAwait(false);
                        await ReplyAsync("Defender:").ConfigureAwait(false);
                        var defenceMessage = await ReplyAsync("```Loading...```").ConfigureAwait(false);
                        _gameState.CurrentWars.Add((segmentId.Value, defendingSegment.Value, direction, attackMessage, new Queue<string>()));
                        _gameState.CurrentWars.Add((defendingSegment.Value, segmentId.Value, opposite, defenceMessage, new Queue<string>()));
                        return;
                    }

                    await ReplyAsync("Good luck fighting... empty space? Try attacking something more interesting please").ConfigureAwait(false);
                }
            }

            private async Task<int?> GetSegmentSelectionAsync()
            {
                var userId = await _userRepository.GetUserId(Context.User).ConfigureAwait(false);
                var segmentId = _gameState.GetUserSegmentSelection(userId);

                if (segmentId.HasValue) return segmentId;

                await ReplyAsync(Responses.SelectSegmentMessage).ConfigureAwait(false);
                return null;

            }
        }
    }
}