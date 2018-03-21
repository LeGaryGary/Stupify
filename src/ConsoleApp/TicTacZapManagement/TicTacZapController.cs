using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.DataModels;
using TicTacZap;
using TicTacZap.Blocks;
using TicTacZap.Blocks.Offence;
using Direction = TicTacZap.Direction;

namespace StupifyConsoleApp.TicTacZapManagement
{
    public class TicTacZapController
    {
        private readonly ILogger<TicTacZapController> _logger;
        private readonly BotContext _db;

        public TicTacZapController(ILogger<TicTacZapController> logger, BotContext db)
        {
            _logger = logger;
            _db = db;
        }

        private Dictionary<int, int?> UserSegmentSelection { get; } = new Dictionary<int, int?>();
        private Dictionary<int, int?> UserTemplateSelection { get; } = new Dictionary<int, int?>();

        internal List<(int attackingSegment, int defendingSegment, Direction direction, IUserMessage attackingmessage, Queue<string> battlefeed)> CurrentWars { get; } = new List<(int, int, Direction, IUserMessage, Queue<string>)>();

        public ShopInventory Shop { get; } = new ShopInventory();

        public int Tick { get; private set; }

        public async Task Run()
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
                    
                    await Wait(timer, 1000);
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.StackTrace);
                throw;
            }
        }

        public async Task<string> RenderInventory(int userId)
        {
            var inventory = await Inventories.GetInventoryAsync(userId);
            return inventory.TextRender();
        }

        public async Task<string> RenderSegmentAsync(int segmentId)
        {
            var resourcesPerTick = await _db.GetSegmentResourcePerTickAsync(segmentId);
            var resources = await _db.GetSegmentResourcesAsync(segmentId);
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

        public async Task<string> RenderSegmentHealthAsync(int segmentId)
        {
            var segment = await Segments.GetAsync(segmentId);
            return segment.HealthTextRender();
        }

        public async Task<bool> SegmentReadyForCombat(int segmentId)
        {
            if (CurrentWars.Any(w => w.attackingSegment == segmentId))
            {
                return false;
            }

            return (await Segments.GetAsync(segmentId)).Blocks.OfType<IOffenceBlock>().Any();
        }

        private async Task PerformAttacks()
        {
            var endedWars = new List<(int, int, Direction, IUserMessage, Queue<string>)>();
            foreach (var war in CurrentWars)
            {
                if (!await _db.Segments.AnyAsync(s => s.SegmentId == war.attackingSegment) ||
                    !await _db.Segments.AnyAsync(s => s.SegmentId == war.defendingSegment))
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
                var attackUser = (await _db.Segments.Include(s => s.User).FirstAsync(s => s.SegmentId == war.attackingSegment)).User;
                var defenceUser = (await _db.Segments.Include(s => s.User).FirstAsync(s => s.SegmentId == war.defendingSegment)).User;
                    var lootAmount = defenceUser.Balance / await _db.Segments.Where(s => s.User.UserId == defenceUser.UserId).CountAsync();
                MakeTransaction(defenceUser, attackUser, lootAmount);

                // Delete defending segment
                await Segments.DeleteSegmentAsync(war.defendingSegment);
                await UniverseController.DeleteSegment(war.defendingSegment);
                var removeSeg = await _db.Segments.FirstAsync(s => s.SegmentId == war.defendingSegment);
                _db.Segments.Remove(removeSeg);
                    
                await _db.SaveChangesAsync();
                    
                await war.attackingmessage.ModifyAsync(m => m.Content = "```Your opponent has been destroyed!```");
            }

            foreach (var war in endedWars)
            {
                CurrentWars.Remove(war);
            }
        }

        private async Task UpdateBalances()
        {
            var bank = await GetBankAsync();

            foreach (var dbSegment in await _db.Segments.Include(s => s.User).ToArrayAsync())
            {
                var amount = bank.Balance / 100000000000m + dbSegment.UnitsPerTick;
                MakeTransaction(bank, dbSegment.User, amount);
            }

            await _db.SaveChangesAsync();
        }

        private async Task Wait(Stopwatch timer, int tickMinTime)
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

        public async Task<bool> SetUserSegmentSelection(int userId, int segmentId)
        {
            if (!UserSegmentSelection.ContainsKey(userId)) UserSegmentSelection.Add(userId, null);
            if (!await _db.Segments.AnyAsync(s => s.User.UserId == userId && s.SegmentId == segmentId)) return false;
            UserSegmentSelection[userId] = segmentId;
            return true;
        }

        public int? GetUserSegmentSelection(int userId)
        {
            if (UserSegmentSelection.ContainsKey(userId)) return UserSegmentSelection[userId];
            UserSegmentSelection.Add(userId, null);
            return null;
        }

        public async Task<bool> SetUserTemplateSelection(int userId, int templateId)
        {
            if (!UserTemplateSelection.ContainsKey(userId)) UserTemplateSelection.Add(userId, null);
            if (!await _db.SegmentTemplates.AnyAsync(s => s.User.UserId == userId && s.SegmentTemplateId == templateId)) return false;
            UserTemplateSelection[userId] = templateId;
            return true;
        }

        public int? GetUserTemplateSelection(int userId)
        {
            if (UserTemplateSelection.ContainsKey(userId)) return UserTemplateSelection[userId];
            UserTemplateSelection.Add(userId, null);
            return null;
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

        public async Task<User> GetBankAsync()
        {
            var bankUser = await _db.Users.FirstOrDefaultAsync(u => u.DiscordUserId == -1)
                            ?? _db.Users.Add(new User
                            {
                                Balance = 100000000000,
                                DiscordUserId = -1,
                            }).Entity;

            await _db.SaveChangesAsync();

            return bankUser;
        }

        public bool MakeTransaction(User fromUser, User toUser, decimal amount)
        {
            if (fromUser.Balance < amount) return false;

            fromUser.Balance -= amount;
            toUser.Balance += amount;

            return true;
        }
    }
}