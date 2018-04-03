using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Stupify.Data.Repositories;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class SegmentTemplate : ModuleBase<CommandContext>
    {
        private readonly TicTacZapController _tacZapController;
        private readonly GameState _gameState;
        private readonly ISegmentRepository _segmentRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IUserRepository _userRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public SegmentTemplate(TicTacZapController tacZapController, GameState gameState, ISegmentRepository segmentRepository, ITemplateRepository templateRepository, IUserRepository userRepository, IInventoryRepository inventoryRepository)
        {
            _tacZapController = tacZapController;
            _gameState = gameState;
            _segmentRepository = segmentRepository;
            _templateRepository = templateRepository;
            _userRepository = userRepository;
            _inventoryRepository = inventoryRepository;
        }

        [Command("SaveTemplate")]
        public async Task SaveTemplateCommand()
        {
            var userId = await _userRepository.GetUserId(Context.User);
            var userSelection = _gameState.GetUserSegmentSelection(userId);
            if (!userSelection.HasValue)
            {
                await ReplyAsync(Responses.SelectSegmentMessage);
                return;
            }

            var segment = await _segmentRepository.GetSegmentAsync(userSelection.Value);
            var templateId = await _templateRepository.NewTemplateAsync(segment, Context.User);

            await ReplyAsync($"Segment saved, Id: {templateId}");
        }

        [Command("Templates")]
        public async Task ShowTemplatesCommand()
        {
            var templates = await _templateRepository.GetTemplatesAsync(Context.User);
            var message = string.Empty;
            foreach (var (id, name) in templates)
            {
                message += name == null
                    ? $"Id: {id}"
                    : $"Id: {id}, {name}";
                message += Environment.NewLine;
            }

            if (message == string.Empty)
            {
                await ReplyAsync($"You have no segment templates, use `{Config.CommandPrefix} SaveTemplate` to save your current segment.");
                return;
            }

            await ReplyAsync("```" + message + "```");
        }

        [Command("Template")]
        public async Task ShowTemplateCommand(int templateId)
        {
            if (!await _templateRepository.UserHasTemplateAsync(Context.User, templateId))
            {
                await ReplyAsync(Responses.TemplateOwnershipProblem);
                return;
            }

            await _tacZapController.ShowTemplateAsync(Context, templateId);
        }

        [Command("ApplyTemplate")]
        public async Task ApplyTemplateCommand()
        {
            var userId = await _userRepository.GetUserId(Context.User);
            var selectedSegment = _gameState.GetUserSegmentSelection(userId);
            var selectedTemplate = _gameState.GetUserTemplateSelection(userId);

            if (!selectedSegment.HasValue)
            {
                await ReplyAsync(Responses.SelectSegmentMessage);
                return;
            }

            if (!selectedTemplate.HasValue)
            {
                await ReplyAsync(Responses.SelectTemplateMessage);
                return;
            }

            var segmentId = selectedSegment.Value;
            var templateId = selectedTemplate.Value;

            var inventory = await _inventoryRepository.GetInventoryAsync(Context.User);
            foreach (var block in await _segmentRepository.ResetSegmentAsync(segmentId))
            {
                inventory.AddBlocks(block.Key, block.Value);
            }
            
            var template = await _templateRepository.GetTemplateAsync(templateId);
            var templateBlocks = new Dictionary<BlockType, int>();

            for (var y = 0; y < template.Blocks.GetLength(1); y++)
            for (var x = 0; x < template.Blocks.GetLength(0); x++) 
            {
                var block = template.Blocks[x, y];
                if (block == null || block.BlockType == BlockType.Controller) continue;

                if (!templateBlocks.ContainsKey(block.BlockType)) templateBlocks.Add(block.BlockType, 0);
                templateBlocks[block.BlockType]++;
            }
            
            var buy = true;
            var notEnough = new Dictionary<BlockType, int>();
            foreach (var templateblock in templateBlocks)
            {
                if (inventory.Blocks.ContainsKey(templateblock.Key) &&
                    inventory.Blocks[templateblock.Key] - templateblock.Value >= 0) continue;

                buy = false;
                notEnough.Add(templateblock.Key, templateblock.Value - inventory.Blocks[templateblock.Key]);
            }

            if (!buy)
            {
                var message = "You don't have enough blocks, you are missing:" + Environment.NewLine;
                foreach (var block in notEnough)
                {
                    message += $"{block.Key} x{block.Value}";
                }

                await ReplyAsync(message);
                return;
            }

            var segment = await _segmentRepository.GetSegmentAsync(segmentId);
            for (var y = 0; y < template.Blocks.GetLength(1); y++)
            for (var x = 0; x < template.Blocks.GetLength(0); x++) 
            {
                var block = template.Blocks[x, y];
                if (block == null || block.BlockType == BlockType.Controller) continue;

                inventory.RemoveBlocks(block.BlockType, 1);
                segment.AddBlock(x, y, block.BlockType);
            }

            await _segmentRepository.SetSegmentAsync(segmentId, segment);
            await _inventoryRepository.SaveInventoryAsync(Context.User, inventory);
            await _tacZapController.ShowSegmentAsync(Context, segmentId);
        }
    }
}
