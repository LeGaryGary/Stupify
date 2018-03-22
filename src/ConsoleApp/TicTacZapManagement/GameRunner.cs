using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StupifyConsoleApp.DataModels;
using TicTacZap;
using TicTacZap.Blocks;
using TicTacZap.Blocks.Offence;
using Direction = TicTacZap.Direction;

namespace StupifyConsoleApp.TicTacZapManagement
{
    public class GameRunner
    {
        private readonly ILogger<GameRunner> _logger;
        private readonly GameState _gameState;

        public GameRunner(ILogger<GameRunner> logger, GameState gameState)
        {
            _logger = logger;
            _gameState = gameState;
        }

        public async Task Run()
        {
            var timer = new Stopwatch();
            timer.Start();
            var dbContextOptions = Config.ServiceProvider.GetService<DbContextOptions<BotContext>>();
            while (true)
            {
                try
                {
                    using (var db = new BotContext(dbContextOptions))
                    {
                        _gameState.Tick++;

                        await PerformAttacks(db);
                        await UpdateBalances(db);

                        await Wait(timer, 1000);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Critical exception thrown!");
                    throw;
                }
            }
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

        private async Task PerformAttacks(BotContext db)
        {
            var endedWars = new List<(int, int, Direction, IUserMessage, Queue<string>)>();
            foreach (var war in _gameState.CurrentWars)
            {
                if (!await db.Segments.AnyAsync(s => s.SegmentId == war.attackingSegment) ||
                    !await db.Segments.AnyAsync(s => s.SegmentId == war.defendingSegment))
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
                    ? $"{_gameState.Tick}:: {Math.Round((decimal) blocksAttacking / attackingSegment.Blocks.OfType<IOffenceBlock>().Count() * 100, 2)} of attacking blocks had enough energy!"
                    : $"{_gameState.Tick}:: There are no offensive blocks!");
                if (war.battlefeed.Count > 5) war.battlefeed.Dequeue();

                if (defendingSegment.Blocks[4, 4] != null)
                {
                    await Segments.SetAsync(war.defendingSegment, defendingSegment);

                    var healthTextRender = attackingSegment.HealthTextRender() + Environment.NewLine;
                    foreach (var str in war.battlefeed)
                    {
                        healthTextRender += str + Environment.NewLine;
                    }
                    if (war.attackingmessage.Content != healthTextRender && _gameState.Tick%2 == 0) await war.attackingmessage.ModifyAsync(m => m.Content = $"```{healthTextRender}```");

                    continue;
                }
                // ---------- Attacking segment has won ----------

                // Get the segment owners and make loot transaction
                var attackUser = (await db.Segments.Include(s => s.User).FirstAsync(s => s.SegmentId == war.attackingSegment)).User;
                var defenceUser = (await db.Segments.Include(s => s.User).FirstAsync(s => s.SegmentId == war.defendingSegment)).User;
                var lootAmount = defenceUser.Balance / await db.Segments.Where(s => s.User.UserId == defenceUser.UserId).CountAsync();
                TicTacZapController.MakeTransaction(defenceUser, attackUser, lootAmount);

                // Delete defending segment
                await Segments.DeleteSegmentAsync(war.defendingSegment);
                await UniverseController.DeleteSegment(war.defendingSegment);
                var removeSeg = await db.Segments.FirstAsync(s => s.SegmentId == war.defendingSegment);
                db.Segments.Remove(removeSeg);
                    
                await db.SaveChangesAsync();
                    
                await war.attackingmessage.ModifyAsync(m => m.Content = "```Your opponent has been destroyed!```");
            }

            foreach (var war in endedWars)
            {
                _gameState.CurrentWars.Remove(war);
            }
        }

        private async Task UpdateBalances(BotContext db)
        {
            var bank = await db.GetBankAsync();

            foreach (var dbSegment in await db.Segments.Include(s => s.User).ToArrayAsync())
            {
                var amount = bank.Balance / 100000000000m + dbSegment.UnitsPerTick;
                TicTacZapController.MakeTransaction(bank, dbSegment.User, amount);
            }

            await db.SaveChangesAsync();
        }
    }
}
