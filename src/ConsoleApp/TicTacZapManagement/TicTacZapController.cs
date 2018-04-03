using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Stupify.Data.Repositories;
using StupifyConsoleApp.Commands;
using StupifyConsoleApp.Commands.Modules.TicTacZap;
using TicTacZap;
using TicTacZap.Blocks.Offence;

namespace StupifyConsoleApp.TicTacZapManagement
{
    public class TicTacZapController
    {
        private readonly ILogger<TicTacZapController> _logger;
        private readonly GameState _gameState;
        private readonly IUserRepository _userRepository;
        private readonly ISegmentRepository _segmentRepository;
        private readonly ITemplateRepository _templateRepository;

        public TicTacZapController(ILogger<TicTacZapController> logger, GameState gameState, IUserRepository userRepository, ISegmentRepository segmentRepository, ITemplateRepository templateRepository)
        {
            _logger = logger;
            _gameState = gameState;
            _userRepository = userRepository;
            _segmentRepository = segmentRepository;
            _templateRepository = templateRepository;
        }

        public ShopInventory Shop { get; } = new ShopInventory();

        public async Task ShowSegmentAsync(ICommandContext context, int segmentId)
        {
            var userId = await _userRepository.GetUserId(context.User);
            if (await _segmentRepository.UserHasSegmentAsync(context.User, segmentId))
            {
                _gameState.SetUserSegmentSelection(userId, segmentId);
                await context.Channel.SendMessageAsync(
                    $"```{await RenderSegmentAsync(segmentId)}```");
            }
            else await context.Channel.SendMessageAsync(Responses.SegmentOwnershipProblem);
        }

        public async Task ShowSegmentAsync(ICommandContext context, int segmentId, Overlay overlay)
        {
            var userId = await _userRepository.GetUserId(context.User);
            if (await _segmentRepository.UserHasSegmentAsync(context.User, segmentId))
            {
                _gameState.SetUserSegmentSelection(userId, segmentId);
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
            var userId = await _userRepository.GetUserId(context.User);
            if (await _templateRepository.UserHasTemplateAsync(context.User, templateId))
            {
                _gameState.SetUserTemplateSelection(userId, templateId);
                await context.Channel.SendMessageAsync($"```{(await _templateRepository.GetTemplateAsync(templateId)).TextRender()}```");
            }
            else await context.Channel.SendMessageAsync(Responses.TemplateOwnershipProblem);
        }

        public async Task<string> RenderSegmentAsync(int segmentId, (int x, int y)? selection = null)
        {
            var resourcesPerTick = await _segmentRepository.GetSegmentResourcePerTickAsync(segmentId);
            var resources = await _segmentRepository.GetSegmentResourcesAsync(segmentId);
            var segment = await _segmentRepository.GetSegmentAsync(segmentId);
            var text = segment.TextRender();

            if (selection != null)
            {
                var i = selection.Value.y * 29 + selection.Value.x * 3 + 1;
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
            var segment = await _segmentRepository.GetSegmentAsync(segmentId);
            return segment.HealthTextRender();
        }

        public async Task<bool> SegmentReadyForCombat(int segmentId)
        {
            return _gameState.CurrentWars.All(w => w.attackingSegment != segmentId) && 
                   (await _segmentRepository.GetSegmentAsync(segmentId)).Blocks.OfType<IOffenceBlock>().Any();
        }

        public async Task<string> RenderBlockInfoAsync(int segmentId, int x, int y)
        {
            var segment = await _segmentRepository.GetSegmentAsync(segmentId);

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
    }
}