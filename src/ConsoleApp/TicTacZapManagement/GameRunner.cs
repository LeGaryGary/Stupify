using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Logging;
using Stupify.Data.Repositories;
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
        private readonly ISegmentRepository _segmentRepository;
        private readonly IUserRepository _userRepository;

        public GameRunner(ILogger<GameRunner> logger, GameState gameState, ISegmentRepository segmentRepository, IUserRepository userRepository)
        {
            _logger = logger;
            _gameState = gameState;
            _segmentRepository = segmentRepository;
            _userRepository = userRepository;
        }

        public async Task Run()
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                _gameState.Tick++;

                await PerformAttacks();
                await _segmentRepository.UpdateBalancesAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unhandled Exception thrown in game loop!");
            }
            await Wait(sw, 1000);
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

        private async Task PerformAttacks()
        {
            var endedWars = new List<(int, int, Direction, IUserMessage, Queue<string>)>();
            foreach (var war in _gameState.CurrentWars)
            {
                try
                {
                    if (!await _segmentRepository.Exists(war.attackingSegment) ||
                    !await _segmentRepository.Exists(war.defendingSegment))
                    {
                        endedWars.Add(war);
                        continue;
                    }

                    var attackingSegment = await _segmentRepository.GetSegmentAsync(war.attackingSegment);
                    var defendingSegment = await _segmentRepository.GetSegmentAsync(war.defendingSegment);

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
                        ? $"{_gameState.Tick}:: {Math.Round((decimal) blocksAttacking / attackingSegment.Blocks.OfType<IOffenceBlock>().Count() * 100, 2)}% of attacking blocks had enough energy!"
                        : $"{_gameState.Tick}:: There are no offensive blocks!");
                    if (war.battlefeed.Count > 5) war.battlefeed.Dequeue();

                    if (defendingSegment.Blocks[4, 4] != null)
                    {
                        await _segmentRepository.SetSegmentAsync(war.defendingSegment, defendingSegment);

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
                    var attackingUser = await _segmentRepository.GetOwner(war.attackingSegment);
                    var defendingUser = await _segmentRepository.GetOwner(war.defendingSegment);

                    var defendingBalance = await _userRepository.BalanceAsync(defendingUser);
                    var defendingSegmentCount = await _segmentRepository.SegmentCountAsync(defendingUser);

                    var lootAmount = defendingBalance / defendingSegmentCount;

                    await _userRepository.BalanceTransferAsync(defendingUser, attackingUser, lootAmount);

                    // Delete defending segment
                    await _segmentRepository.DeleteSegmentAsync(war.defendingSegment);
                    
                    await war.attackingmessage.ModifyAsync(m => m.Content = "```Your opponent has been destroyed!```");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "The war {Attacking} => {Defending} has encountered an error", war.attackingSegment, war.defendingSegment);
                    endedWars.Add(war);
                }
            }

            foreach (var war in endedWars)
            {
                _gameState.CurrentWars.Remove(war);
            }
        }
    }
}
