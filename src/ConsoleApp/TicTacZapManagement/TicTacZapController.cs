using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.DataModels;
using TicTacZap;
using TicTacZap.Blocks;
using TicTacZap.Blocks.Offence;
using Direction = TicTacZap.Direction;

namespace StupifyConsoleApp.TicTacZapManagement
{
    internal static class TicTacZapController
    {
        static TicTacZapController()
        {
        }

        private static Dictionary<int, int?> UserSegmentSelection { get; } = new Dictionary<int, int?>();
        private static Dictionary<int, int?> UserTemplateSelection { get; } = new Dictionary<int, int?>();

        internal static List<(int attackingSegment, int defendingSegment, Direction direction, IUserMessage attackingmessage, Queue<string> battlefeed)> CurrentWars { get; } = new List<(int, int, Direction, IUserMessage, Queue<string>)>();

        public static ShopInventory Shop { get; } = new ShopInventory();

        public static int Tick { get; private set; }

        public static async Task Run()
        {
            try
            {
                var timer = new Stopwatch();
                timer.Start();
                while (true)
                {
                    Tick++;

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
            var resourcesPerTick = await db.GetSegmentResourcePerTickAsync(segmentId);
            var resources = await db.GetSegmentResourcesAsync(segmentId);
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

        public static async Task<string> RenderSegmentHealthAsync(int segmentId)
        {
            var segment = await Segments.GetAsync(segmentId);
            return segment.HealthTextRender();
        }

        public static async Task<bool> SegmentReadyForCombat(int segmentId)
        {
            if (CurrentWars.Any(w => w.attackingSegment == segmentId))
            {
                return false;
            }

            return (await Segments.GetAsync(segmentId)).Blocks.OfType<IOffenceBlock>().Any();
        }

        private static async Task PerformAttacks()
        {
            using (var context = new BotContext())
            {
                var endedWars = new List<(int, int, Direction, IUserMessage, Queue<string>)>();
                foreach (var war in CurrentWars)
                {
                    if (!await context.Segments.AnyAsync(s => s.SegmentId == war.attackingSegment) ||
                        !await context.Segments.AnyAsync(s => s.SegmentId == war.defendingSegment))
                    {
                        endedWars.Add(war);
                        continue;
                    }

                    var attackingSegment = await Segments.GetAsync(war.attackingSegment);
                    var defendingSegment = await Segments.GetAsync(war.defendingSegment);

                    var remainingTickEnergy = attackingSegment.ResourcePerTick(Resource.Energy);
                    var blocksAttacking = 0;
                    foreach (var block in attackingSegment.Blocks)
                    {
                        if (block is IEnergyConsumer energyConsumer)
                        {
                            if (energyConsumer.EnergyConsumption > remainingTickEnergy) continue;
                            remainingTickEnergy -= energyConsumer.EnergyConsumption;
                        }

                        if (!(block is IOffenceBlock attackBlock)) continue;

                        attackBlock.AttackSegment(war.direction, defendingSegment);
                        blocksAttacking++;
                    }

                    war.battlefeed.Enqueue(attackingSegment.Blocks.OfType<IOffenceBlock>().Any()
                        ? $"{Tick}:: {Math.Round((decimal) blocksAttacking / attackingSegment.Blocks.OfType<IOffenceBlock>().Count() * 100, 2)} of attacking blocks had enough energy!"
                        : $"{Tick}:: There are no offensive blocks!");
                    if (war.battlefeed.Count > 5) war.battlefeed.Dequeue();

                    if (defendingSegment.Blocks[4, 4] != null)
                    {
                        await Segments.SetAsync(war.defendingSegment, defendingSegment);

                        var healthTextRender = attackingSegment.HealthTextRender() + Environment.NewLine;
                        foreach (var str in war.battlefeed)
                        {
                            healthTextRender += str + Environment.NewLine;
                        }
                        if (war.attackingmessage.Content != healthTextRender && Tick%2 == 0) await war.attackingmessage.ModifyAsync(m => m.Content = $"```{healthTextRender}```");

                        continue;
                    }
                    // ---------- Attacking segment has won ----------

                    // Get the segment owners and make loot transaction
                    var attackUser = (await context.Segments.Include(s => s.User).FirstAsync(s => s.SegmentId == war.attackingSegment)).User;
                    var defenceUser = (await context.Segments.Include(s => s.User).FirstAsync(s => s.SegmentId == war.defendingSegment)).User;
                     var lootAmount = defenceUser.Balance / await context.Segments.Where(s => s.User.UserId == defenceUser.UserId).CountAsync();
                    MakeTransaction(defenceUser, attackUser, lootAmount);

                    // Delete defending segment
                    await Segments.DeleteSegmentAsync(war.defendingSegment);
                    await UniverseController.DeleteSegment(war.defendingSegment);
                    var removeSeg = await context.Segments.FirstAsync(s => s.SegmentId == war.defendingSegment);
                    context.Segments.Remove(removeSeg);
                    
                    await context.SaveChangesAsync();
                    
                    await war.attackingmessage.ModifyAsync(m => m.Content = "```Your opponent has been destroyed!```");
                }

                foreach (var war in endedWars)
                {
                    CurrentWars.Remove(war);
                }
            }
        }

        private static async Task UpdateBalances()
        {
            using (var db = new BotContext())
            {
                var bank = await GetBankAsync(db);

                foreach (var dbSegment in await db.Segments.Include(s => s.User).ToArrayAsync())
                {
                    var amount = bank.Balance / 100000000000m + dbSegment.UnitsPerTick;
                    MakeTransaction(bank, dbSegment.User, amount);
                }

                await db.SaveChangesAsync();
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

        public static async Task<string> RenderBlockInfoAsync(int segmentSelectionId, int x, int y)
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

        public static async Task<User> GetBankAsync(BotContext context)
        {
            var bankUser = await context.Users.FirstOrDefaultAsync(u => u.DiscordUserId == -1)
                            ?? context.Users.Add(new User
                            {
                                Balance = 100000000000,
                                DiscordUserId = -1,
                            }).Entity;

            await context.SaveChangesAsync();

            return bankUser;
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