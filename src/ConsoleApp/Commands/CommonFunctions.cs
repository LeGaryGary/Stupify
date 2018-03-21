﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StupifyConsoleApp.Commands.Modules.TicTacZap;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap;
using TicTacZap.Blocks;
using Segments = StupifyConsoleApp.TicTacZapManagement.Segments;

namespace StupifyConsoleApp.Commands
{
    public static class CommonFunctions
    {
        private static TicTacZapController TicTacZapController => Config.ServiceProvider.GetService<TicTacZapController>();

        public static async Task<User> GetUserAsync(this StupifyModuleBase moduleBase)
        {
            var userId = (long) moduleBase.Context.User.Id;
            return await moduleBase.Db.Users.FirstAsync(u => u.DiscordUserId == userId);
        }

        public static async Task<IEnumerable<DataModels.Segment>> GetSegments(this StupifyModuleBase moduleBase)
        {
            var user = await GetUserAsync(moduleBase);
            return moduleBase.Db.Segments.Where(s => s.User.UserId == user.UserId);
        }

        public static async Task<bool> UserHasSegmentAsync(this StupifyModuleBase moduleBase, int segmentId)
        {
            return (await GetSegments(moduleBase)).Select(s => s.SegmentId).Contains(segmentId);
        }

        public static async Task<int> SegmentCountAsync(this StupifyModuleBase moduleBase)
        {
            var user = await moduleBase.GetUserAsync();
            return await moduleBase.Db.Segments.Where(s => s.User.UserId == user.UserId).CountAsync();
        }

        public static async Task ShowSegmentAsync(this StupifyModuleBase moduleBase, int segmentId)
        {
            await TicTacZapController.SetUserSegmentSelection((await moduleBase.GetUserAsync()).UserId, segmentId);
            await moduleBase.Context.Channel.SendMessageAsync(
                $"```{await TicTacZapController.RenderSegmentAsync(segmentId)}```");
        }

        public static async Task ShowSegmentAsync(this StupifyModuleBase moduleBase, int segmentId, Overlay overlay)
        {
            await TicTacZapController.SetUserSegmentSelection((await moduleBase.GetUserAsync()).UserId, segmentId);
            switch (overlay)
            {
                case Overlay.Health:
                    await moduleBase.Context.Channel.SendMessageAsync($"```{await TicTacZapController.RenderSegmentHealthAsync(segmentId)}```");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(overlay), overlay, null);
            }
        }

        public static async Task ShowTemplateAsync(this StupifyModuleBase moduleBase, int templateId)
        {
            await TicTacZapController.SetUserTemplateSelection((await moduleBase.GetUserAsync()).UserId, templateId);

            await moduleBase.Context.Channel.SendMessageAsync(
                $"```{(await SegmentTemplates.GetAsync(templateId)).TextRender()}```");
        }

        public static async Task UpdateDbSegmentOutput(this StupifyModuleBase moduleBase, int segmentId)
        {
            var dbSegment = await moduleBase.Db.Segments.FirstAsync(s => s.SegmentId == segmentId);
            var segmentOutput = await Segments.GetOutput(segmentId);
            
            dbSegment.EnergyPerTick = segmentOutput[Resource.Energy];

            await moduleBase.Db.SaveChangesAsync();
        }

        public static async Task ClearSegmentToInventory(this StupifyModuleBase moduleBase, int segmentId, int userId)
        {
            var blocks = await Segments.ResetSegmentAsync(segmentId);
            foreach (var pair in blocks)
            {
                if (pair.Value > 0)
                {
                    await Inventories.AddToInventoryAsync(pair.Key, pair.Value, userId);
                }
            }
        }

        public static async Task AddBlock(this StupifyModuleBase moduleBase, int segmentId, int userId, int x, int y, BlockType blockType, bool show = true)
        {
            if (await Inventories.RemoveFromInventoryAsync(blockType, 1, userId))
            {
                if (!await Segments.AddBlockAsync(segmentId, x - 1, y - 1, blockType))
                    await Inventories.AddToInventoryAsync(blockType, 1, userId);
                await UpdateDbSegmentOutput(moduleBase, segmentId);
                if(show) await ShowSegmentAsync(moduleBase, segmentId);
                return;
            }

            await moduleBase.Context.Channel.SendMessageAsync(Responses.ShopAdvisoryMessage);
        }

        public static async Task RemoveBlock(this StupifyModuleBase moduleBase, int segmentId, int x, int y, bool show = true)
        {
            if (await UserHasSegmentAsync(moduleBase, segmentId))
            {
                var blockType = await Segments.DeleteBlockAsync(segmentId, x - 1, y - 1);

                if (blockType != null)
                    await Inventories.AddToInventoryAsync(blockType.Value, 1, (await GetUserAsync(moduleBase)).UserId);
                await UpdateDbSegmentOutput(moduleBase, segmentId);
                if(show) await ShowSegmentAsync(moduleBase, segmentId);
                return;
            }

            await moduleBase.Context.Channel.SendMessageAsync(Responses.SegmentOwnershipProblem);
        }
    }
}