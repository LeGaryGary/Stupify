using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.DataModels;
using TicTacZap;
using TicTacZap.Blocks.Offence;

namespace StupifyConsoleApp.TicTacZapManagement
{
    internal static class TicTacZapController
    {
        static TicTacZapController()
        {
        }

        private static Dictionary<int, int?> UserSegmentSelection { get; } = new Dictionary<int, int?>();
        private static Dictionary<int, int?> UserTemplateSelection { get; } = new Dictionary<int, int?>();

        internal static List<(int attackingSegment, int defendingSegment, Direction direction)> CurrentWars { get; } = new List<(int, int, Direction)>();

        public static ShopInventory Shop { get; } = new ShopInventory();

        public static async Task Run()
        {
            try
            {
                var timer = new Stopwatch();
                timer.Start();
                while (true)
                {
                    await PerformAttacks();
                    await UpdateBalances();

                    await timer.Wait(1000);
                }
            }
            catch (Exception e)
            {
                await ClientManager.LogAsync(e.ToString());
                throw;
            }
        }

        public static async Task<string> RenderInventory(int userId)
        {
            var inventory = await Inventories.GetInventoryAsync(userId);
            return inventory.TextRender();
        }

        public static async Task<string> RenderSegmentAsync(int segmentId, BotContext db)
        {
            var resourcesPerTick = db.GetSegmentResourcePerTick(segmentId);
            var resources = db.GetSegmentResources(segmentId);
            var segment = await Segments.GetAsync(segmentId);
            var text = segment.TextRender();

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

        private static async Task PerformAttacks()
        {
            using (var context = new BotContext())
            {
                foreach (var war in CurrentWars)
                {
                    if (!await context.Segments.AnyAsync(s => s.SegmentId == war.attackingSegment) ||
                        !await context.Segments.AnyAsync(s => s.SegmentId == war.defendingSegment))
                    {
                        CurrentWars.Remove(war);
                        continue;
                    }

                    var attackingSegment = await Segments.GetAsync(war.attackingSegment);
                    var defendingSegment = await Segments.GetAsync(war.defendingSegment);

                    foreach (var block in attackingSegment.Blocks)
                    {
                        if (block is IOffenceBlock attackBlock)
                        {
                            attackBlock.AttackSegment(war.direction, defendingSegment);
                        }
                    }

                    if (defendingSegment.Blocks[4, 4] != null) continue;

                    await Segments.DeleteSegmentAsync(war.defendingSegment);
                    await UniverseController.DeleteSegment(war.defendingSegment);
                
                    var removeSeg = await context.Segments.FirstAsync(s => s.SegmentId == war.defendingSegment);
                    context.Segments.Remove(removeSeg);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task UpdateBalances()
        {
            using (var db = new BotContext())
            {
                var segments = await db.Segments.Include(s => s.User).ToArrayAsync();
                foreach (var segment in segments)
                    try
                    {
                        var user = await db.Users.FirstAsync(u => u.UserId == segment.User.UserId);

                        user.Balance += segment.UnitsPerTick;
                        segment.Energy += segment.EnergyPerTick;

                        await db.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        await ClientManager.LogAsync(e.ToString());
                        throw;
                    }
            }
        }

        private static async Task Wait(this Stopwatch timer, int tickMinTime)
        {
            while (true)
            {
                if (timer.ElapsedMilliseconds > tickMinTime)
                {
                    timer.Restart();
                    break;
                }

                await Task.Delay(50);
            }
        }

        public static async Task<bool> SetUserSegmentSelection(int userId, int segmentId, BotContext db)
        {
            if (!UserSegmentSelection.ContainsKey(userId)) UserSegmentSelection.Add(userId, null);
            if (!await db.Segments.AnyAsync(s => s.User.UserId == userId && s.SegmentId == segmentId)) return false;
            UserSegmentSelection[userId] = segmentId;
            return true;
        }

        public static int? GetUserSegmentSelection(int userId)
        {
            if (UserSegmentSelection.ContainsKey(userId)) return UserSegmentSelection[userId];
            UserSegmentSelection.Add(userId, null);
            return null;
        }

        public static async Task<bool> SetUserTemplateSelection(int userId, int templateId, BotContext db)
        {
            if (!UserTemplateSelection.ContainsKey(userId)) UserTemplateSelection.Add(userId, null);
            if (!await db.SegmentTemplates.AnyAsync(s => s.User.UserId == userId && s.SegmentTemplateId == templateId)) return false;
            UserTemplateSelection[userId] = templateId;
            return true;
        }

        public static int? GetUserTemplateSelection(int userId)
        {
            if (UserTemplateSelection.ContainsKey(userId)) return UserTemplateSelection[userId];
            UserTemplateSelection.Add(userId, null);
            return null;
        }
    }
}