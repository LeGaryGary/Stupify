using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.DataModels;
using TicTacZap;
using TicTacZap.Segment.Blocks;

namespace StupifyConsoleApp.TicTacZap
{
    internal static class TicTacZapController
    {
        private static Dictionary<int, int?> UserSegmentSelection { get; } = new Dictionary<int, int?>(); 

        public static ShopInventory Shop { get; } = new ShopInventory();

        static TicTacZapController()
        {
            
        }

        public static async Task Run()
        {
            try
            {
                var timer = new Stopwatch();
                timer.Start();
                while (true)
                {
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

        private static async Task UpdateBalances()
        {
            using (var db = new BotContext())
            {
                foreach (var segment in await db.Segments.ToArrayAsync())
                {
                    try
                    {
                        var user = await db.Users.FirstAsync(u => u.UserId == segment.UserId);

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
            if (!await db.Segments.AnyAsync(s => s.UserId == userId && s.SegmentId == segmentId)) return false;
            UserSegmentSelection[userId] = segmentId;
            return true;

        }

        public static int? GetUserSelection(int userId)
        {
            if (UserSegmentSelection.ContainsKey(userId)) return UserSegmentSelection[userId];
            UserSegmentSelection.Add(userId, null);
            return null;
        }

    }
}
