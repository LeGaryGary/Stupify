using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Stupify.Data;
using Stupify.Data.Repositories;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class Segments : ModuleBase<CommandContext>
    {
        private readonly ILogger<Segments> _logger;
        private readonly ISegmentRepository _segmentRepository;
        private readonly IUniverseRepository _universeRepository;

        public Segments(ILogger<Segments> logger, ISegmentRepository segmentRepository, IUniverseRepository universeRepository)
        {
            _logger = logger;
            _segmentRepository = segmentRepository;
            _universeRepository = universeRepository;
        }

        [Command("Segments")]
        public async Task ListSegments()
        {
            var segments = await _segmentRepository.GetSegmentIds(Context.User);
            var renderSegmentList = await RenderSegmentListAsync(segments);
            if (renderSegmentList == string.Empty)
                await ReplyAsync($"You don't have any segments, buy your first one: `{Config.CommandPrefix} Segment Buy`");
            else
                await ReplyAsync(renderSegmentList + Environment.NewLine + $"Use `{Config.CommandPrefix} Segment [segmentId]` to select and show a segment!");
        }

        private async Task<string> RenderSegmentListAsync(IEnumerable<int> segmentIds)
        {
            var render = string.Empty;

            foreach (var segmentId in segmentIds)
            {
                var location = await _universeRepository.FindAsync(segmentId);
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

            public SegmentModule(BotContext db, TicTacZapController tacZapController, SegmentEditReactionHandler editReactionHandler, GameState gameState) : base(db)
            public SegmentModule(TicTacZapController tacZapController, GameState gameState, IUserRepository userRepository, ISegmentRepository segmentRepository, IInventoryRepository inventoryRepository, IUniverseRepository universeRepository)
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
            public async Task Segment()
            {
                var userId = await _userRepository.GetUserId(Context.User);
                var segmentId = _gameState.GetUserSegmentSelection(userId);
                if (!segmentId.HasValue)
                {
                    await ReplyAsync(Responses.SelectSegmentMessage);
                    return;
                }
                await _tacZapController.ShowSegmentAsync(Context, segmentId.Value);
            }

            [Command]
            public async Task Segment(int segmentId)
            {
                if (await _segmentRepository.UserHasSegmentAsync(Context.User, segmentId))
                {
                    await _tacZapController.ShowSegmentAsync(Context, segmentId);
                    return;
                }

                await ReplyAsync(Responses.SegmentOwnershipProblem);
            }

            [Command]
            public async Task Segment(int segmentId, Overlay overlayType)
            {
                if (await _segmentRepository.UserHasSegmentAsync(Context.User, segmentId))
                {
                    await _tacZapController.ShowSegmentAsync(Context, segmentId, overlayType);
                    return;
                }

                await ReplyAsync(Responses.SegmentOwnershipProblem);
            }

            [Command("Buy")]
            public async Task BuySegment()
            {
                var price = SegmentPrice(await _segmentRepository.SegmentCountAsync(Context.User));
                if (!await _userRepository.UserToBankTransferAsync(Context.User, price))
                {
                    await ReplyAsync(Responses.NotEnoughUnits(price));
                    return;
                }

                var segment = await _segmentRepository.NewSegmentAsync(Context.User);

                await ReplyAsync($"You have purchased a segment!\r\nId: {segment.segmentId}\r\nCoordinates: {segment.coords.Value.x+1}, {segment.coords.Value.y+1}");
            }

            [Command("Reset")]
            public async Task ResetSegment()
            {
                var userId = await _userRepository.GetUserId(Context.User);
                var segmentId = _gameState.GetUserSegmentSelection(userId);
                if (!segmentId.HasValue)
                {
                    await ReplyAsync(Responses.SelectSegmentMessage);
                    return;
                }

                var blocks = await _segmentRepository.ResetSegmentAsync(segmentId.Value);

                foreach (var type in blocks)
                    if (type.Value > 0)
                        await _inventoryRepository.AddToInventoryAsync(type.Key, type.Value, Context.User);
                
                await ReplyAsync($"segment {segmentId} was reset!");
            }

            [Command("Edit")]
            public async Task EditSegmentCommand(int segmentId)
            {
                if (!await this.UserHasSegmentAsync(segmentId))
                {
                    await ReplyAsync(Responses.SegmentOwnershipProblem);
                    return;
                }

                var msg = await ReplyAsync($"```Hang on...```");
                await _editReactionHandler.NewOwner(msg, segmentId, Context.User.Id, (await this.GetUserAsync()).UserId);
            }

            [Command("Delete")]
            public async Task DeleteSegmentCommand()
            {
                var userId = await _userRepository.GetUserId(Context.User);
                var segmentId = _gameState.GetUserSegmentSelection(userId);
                if (!segmentId.HasValue)
                {
                    await ReplyAsync(Responses.SelectSegmentMessage);
                    return;
                }

                var locationNullable = await _segmentRepository.DeleteSegmentAsync(segmentId.Value);
                if (locationNullable.HasValue)
                {
                    await ReplyAsync($"It's gone...\r\nId: {segmentId}\r\nCoordinates: {locationNullable.Value.x + 1}, {locationNullable.Value.y + 1}");
                    return;
                }

                await ReplyAsync("Something went wrong, we couldn't find your segment in the universe, how odd");
            }

            [Command("Attack")]
            public async Task AttackSegmentCommand(Direction direction)
            {
                var userId = await _userRepository.GetUserId(Context.User);
                var segment = _gameState.GetUserSegmentSelection(userId);
                if (!segment.HasValue)
                {
                    await ReplyAsync(Responses.SelectSegmentMessage);
                    return;
                }

                if (!await _tacZapController.SegmentReadyForCombat(segment.Value))
                {
                    await ReplyAsync("This segment isn't ready for combat (needs to have offensive blocks and not already be in combat)");
                    return;
                }

                var defendingSegment = await _universeRepository.GetAdjacentSegmentInDirectionAsync(segment.Value, direction);
                if (defendingSegment.HasValue)
                {
                    var opposite = direction.Opposite();
                    await ReplyAsync("Attacker:");
                    var attackMessage = await ReplyAsync("```Loading...```");
                    await ReplyAsync("Defender:");
                    var defenceMessage = await ReplyAsync("```Loading...```");
                    _gameState.CurrentWars.Add((segment.Value, defendingSegment.Value, direction, attackMessage, new Queue<string>()));
                    _gameState.CurrentWars.Add((defendingSegment.Value, segment.Value, opposite, defenceMessage, new Queue<string>()));
                    return;
                }

                await ReplyAsync("Good luck fighting... empty space? Try attacking something more interesting please");
            }
        }
    }
}