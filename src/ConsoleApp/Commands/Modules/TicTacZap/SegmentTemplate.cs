using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class SegmentTemplate : StupifyModuleBase
    {
        private readonly TicTacZapController _tacZapController;

        public SegmentTemplate(BotContext db, TicTacZapController tacZapController) : base(db)
        {
            _tacZapController = tacZapController;
        }

        [Command("SaveTemplate")]
        public async Task SaveTemplateCommand()
        {
            var user = await this.GetUserAsync();
            var userSelection = _tacZapController.GetUserSegmentSelection(user.UserId);
            if (userSelection == null)
            {
                await ReplyAsync(Responses.SelectSegmentMessage);
                return;
            }

            var templateId = await Db.NewTemplateAsync(user);
            var segment = await TicTacZapManagement.Segments.GetAsync((int)userSelection);
            await SegmentTemplates.SaveAsync(templateId, segment);

            await ReplyAsync($"Segment saved, Id: {templateId}");
        }

        [Command("Templates")]
        public async Task ShowTemplatesCommand()
        {
            var templates = await GetTemplatesAsync();
            var message = string.Empty;
            foreach (var template in templates)
            {
                message += template.Name == null
                    ? $"Id: {template.SegmentTemplateId}"
                    : $"Id: {template.SegmentTemplateId}, {template.Name}";
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
            var user = await this.GetUserAsync();
            var dbTemplate = await Db.SegmentTemplates.FirstOrDefaultAsync(st => st.SegmentTemplateId == templateId && st.User.UserId == user.UserId);
            if (dbTemplate == null)
            {
                await ReplyAsync(Responses.TemplateOwnershipProblem);
                return;
            }

            await this.ShowTemplateAsync(templateId);
        }

        [Command("ApplyTemplate")]
        public async Task ApplyTemplateCommand()
        {
            var user = await this.GetUserAsync();
            var selectedSegment = _tacZapController.GetUserSegmentSelection(user.UserId);
            var selectedTemplate = _tacZapController.GetUserTemplateSelection(user.UserId);

            if (selectedSegment == null)
            {
                await ReplyAsync(Responses.SelectSegmentMessage);
                return;
            }

            if (selectedTemplate == null)
            {
                await ReplyAsync(Responses.SelectTemplateMessage);
                return;
            }

            var segmentId = (int)selectedSegment;
            var templateId = (int)selectedTemplate;

            await this.ClearSegmentToInventory(segmentId, user.UserId);
            
            var template = await SegmentTemplates.GetAsync(templateId);
            var templateBlocks = new Dictionary<BlockType, int>();

            for (var y = 0; y < template.Blocks.GetLength(1); y++)
            for (var x = 0; x < template.Blocks.GetLength(0); x++) 
            {
                var block = template.Blocks[x, y];
                if (block == null || block.BlockType == BlockType.Controller) continue;

                if (!templateBlocks.ContainsKey(block.BlockType)) templateBlocks.Add(block.BlockType, 0);
                templateBlocks[block.BlockType]++;
            }

            var inv = await Inventories.GetInventoryAsync(user.UserId);
            var buy = true;
            var notEnough = new Dictionary<BlockType, int>();
            foreach (var templateblock in templateBlocks)
            {
                if (inv.Blocks.ContainsKey(templateblock.Key) &&
                    inv.Blocks[templateblock.Key] - templateblock.Value >= 0) continue;

                buy = false;
                notEnough.Add(templateblock.Key, templateblock.Value - inv.Blocks[templateblock.Key]);
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

            for (var y = 0; y < template.Blocks.GetLength(1); y++)
            for (var x = 0; x < template.Blocks.GetLength(0); x++) 
            {
                var block = template.Blocks[x, y];
                if (block == null || block.BlockType == BlockType.Controller) continue;

                inv.RemoveBlocks(block.BlockType, 1);
                await TicTacZapManagement.Segments.AddBlockAsync(segmentId, x, y, block.BlockType);
            }

            await Inventories.SaveInventoryAsync(user.UserId, inv);
            await this.UpdateDbSegmentOutput(segmentId);
        }

        

        private async Task<List<DataModels.SegmentTemplate>> GetTemplatesAsync()
        {
            var user = await this.GetUserAsync();
            return await Db.SegmentTemplates.Where(st => st.User.UserId == user.UserId).ToListAsync();
        }
    }
}
