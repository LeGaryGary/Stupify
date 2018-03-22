using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using StupifyConsoleApp.Commands;
using StupifyConsoleApp.Commands.Modules.TicTacZap;
using StupifyConsoleApp.DataModels;
using TicTacZap;
using TicTacZap.Blocks.Offence;

namespace StupifyConsoleApp.TicTacZapManagement
{
    public class TicTacZapController
    {
        private readonly ILogger<TicTacZapController> _logger;
        private readonly BotContext _db;
        private readonly GameState _gameState;

        public TicTacZapController(ILogger<TicTacZapController> logger, BotContext db, GameState gameState)
        {
            _logger = logger;
            _db = db;
            _gameState = gameState;
        }

        public ShopInventory Shop { get; } = new ShopInventory();

        public async Task ShowSegmentAsync(ICommandContext context, int segmentId)
        {
            if (await _gameState.SetUserSegmentSelection((await _db.GetDbUser(context.User)).UserId, segmentId, _db))
            {
                await context.Channel.SendMessageAsync(
                    $"```{await RenderSegmentAsync(segmentId)}```");
            }
            else await context.Channel.SendMessageAsync(Responses.SegmentOwnershipProblem);
        }

        public async Task ShowSegmentAsync(ICommandContext context, int segmentId, Overlay overlay)
        {
            if (await _gameState.SetUserSegmentSelection((await _db.GetDbUser(context.User)).UserId, segmentId, _db))
            {
                switch (overlay)
                {
                    case Overlay.Health:
                        await context.Channel.SendMessageAsync($"```{await RenderSegmentHealthAsync(segmentId)}```");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(overlay), overlay, null);
                }
            }
            else await context.Channel.SendMessageAsync(Responses.SegmentOwnershipProblem);
        }

        public async Task ShowTemplateAsync(ICommandContext context, int templateId)
        {
            if (await _gameState.SetUserTemplateSelection((await _db.GetDbUser(context.User)).UserId, templateId, _db))
            {
                await context.Channel.SendMessageAsync(
                    $"```{(await SegmentTemplates.GetAsync(templateId)).TextRender()}```");
            }
            else await context.Channel.SendMessageAsync(Responses.TemplateOwnershipProblem);
        }

        public async Task<string> RenderInventory(int userId)
        {
            var inventory = await Inventories.GetInventoryAsync(userId);
            return inventory.TextRender();
        }

        public async Task<string> RenderSegmentAsync(int segmentId, Tuple<int, int> selection = null)
        {
            var resourcesPerTick = await _db.GetSegmentResourcePerTickAsync(segmentId);
            var resources = await _db.GetSegmentResourcesAsync(segmentId);
            var segment = await Segments.GetAsync(segmentId);
            var text = segment.TextRender();

            if (selection != null)
            {
                var i = selection.Item1 * 29 + selection.Item2 * 3 + 1;
                var tmp = new StringBuilder(text) {[i] = '#'};
                text = tmp.ToString();

            }

            foreach (var resource in resourcesPerTick)
            {
                text += Environment.NewLine;
                text += $"{resource.Key.ToString()} per tick: {resource.Value}";
            }

            foreach (var resource in resources)
            {
                text += Environment.NewLine;
                text += $"{resource.Key.ToString()} in segment: {resource.Value}";
            }

            return text;
        }

        public async Task<string> RenderSegmentHealthAsync(int segmentId)
        {
            var segment = await Segments.GetAsync(segmentId);
            return segment.HealthTextRender();
        }

        public async Task<bool> SegmentReadyForCombat(int segmentId)
        {
            return _gameState.CurrentWars.All(w => w.attackingSegment != segmentId) && 
                   (await Segments.GetAsync(segmentId)).Blocks.OfType<IOffenceBlock>().Any();
        }

        public async Task<string> RenderBlockInfoAsync(int segmentSelectionId, int x, int y)
        {
            var segment = await Segments.GetAsync(segmentSelectionId);

            if (x < 0 || y < 0 ||
                x >= segment.Blocks.GetLength(0) ||
                y >= segment.Blocks.GetLength(1))
            {
                return null;
            }

            var block = segment.Blocks[x, y];
            if (block == null) return null;

            var info = block.RenderBlockInfo();

            var str = string.Empty;
            str += $"Block: {info.Type}" + Environment.NewLine;
            str += $"Health: {info.Health}/{info.MaxHealth}";

            return str;
        }

        public static bool MakeTransaction(User fromUser, User toUser, decimal amount)
        {
            if (fromUser.Balance < amount) return false;

            fromUser.Balance -= amount;
            toUser.Balance += amount;

            return true;
        }
    }
}