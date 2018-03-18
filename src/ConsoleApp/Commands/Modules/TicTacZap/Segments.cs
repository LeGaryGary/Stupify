using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap;
using Segment = StupifyConsoleApp.DataModels.Segment;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class Segments : StupifyModuleBase
    {
        [Command("Segments")]
        public async Task ListSegments()
        {
            var segments = await this.GetSegments();
            var renderSegmentList = RenderSegmentList(segments);
            if (renderSegmentList == string.Empty)
                await ReplyAsync(
                    $"You don't have any segments, buy your first one: `{Config.CommandPrefix} Segment Buy`");
            else
                await ReplyAsync(renderSegmentList);
        }

        private static string RenderSegmentList(IEnumerable<Segment> segments)
        {
            return segments.Aggregate(
                string.Empty,
                (current, segment) => 
                    current + ($"Segment Id: {segment.SegmentId} Output: {segment.UnitsPerTick}" + Environment.NewLine));
        }

        private static decimal SegmentPrice(int segmentCount)
        {
            return segmentCount * 100;
        }

        [Group("Segment")]
        public class SegmentModule : StupifyModuleBase
        {
            [Command]
            public async Task Segment(int segmentId)
            {
                if (await this.UserHasSegmentAsync(segmentId))
                {
                    await this.ShowSegmentAsync(segmentId);
                    return;
                }

                await ReplyAsync(Responses.SegmentOwnershipProblem);
            }

            [Command]
            public async Task Segment(int segmentId, Overlay overlayType)
            {
                if (await this.UserHasSegmentAsync(segmentId))
                {
                    await this.ShowSegmentAsync(segmentId, overlayType);
                    return;
                }

                await ReplyAsync(Responses.SegmentOwnershipProblem);
            }

            [Command("Buy")]
            public async Task BuySegment()
            {
                var user = await this.GetUserAsync();

                //if (await Db.Segments.Where(s => s.User.UserId == user.UserId).CountAsync() >= 50)
                //{
                //    await ReplyAsync("You have reached the maximum number of segments, unable to add more");
                //}

                var price = SegmentPrice(await this.SegmentCountAsync());
                if (!TicTacZapController.MakeTransaction(await this.GetUserAsync(), await TicTacZapController.GetBankAsync(Db), price))
                {
                    await ReplyAsync(Responses.NotEnoughUnits(price));
                    return;
                }
                await Db.SaveChangesAsync();

                var segment = await NewSegment(user.UserId);

                await ReplyAsync(
                    $"You have purchased a segment!\r\nId: {segment.segmentId}\r\nCoordinates: {segment.coords.Value.x+1}, {segment.coords.Value.y+1}");
            }

            [Command("Reset")]
            public async Task ResetSegment(int segmentId)
            {
                if (!await this.UserHasSegmentAsync(segmentId))
                {
                    await ReplyAsync(Responses.SegmentOwnershipProblem);
                    return;
                }

                var user = await this.GetUserAsync();
                var blocks = await TicTacZapManagement.Segments.ResetSegmentAsync(segmentId);

                foreach (var type in blocks)
                    if (type.Value > 0)
                        await Inventories.AddToInventoryAsync(type.Key, type.Value, user.UserId);

                await Db.SaveChangesAsync();
                await this.UpdateDbSegmentOutput(segmentId);
                await ReplyAsync($"segment {segmentId} was reset!");
            }

            [Command("Delete")]
            public async Task DeleteSegmentCommand(int segmentId)
            {
                if (!await this.UserHasSegmentAsync(segmentId))
                {
                    await ReplyAsync(Responses.SegmentOwnershipProblem);
                    return;
                }

                var locationNullable = await DeleteSegment(segmentId);
                if (locationNullable.HasValue)
                {
                    await ReplyAsync($"It's gone...\r\nId: {segmentId}\r\nCoordinates: {locationNullable.Value.x + 1}, {locationNullable.Value.y + 1}");
                    return;
                }

                await ReplyAsync("Something went wrong, we couldn't find your segment in the universe, how odd");
            }

            [Command("Attack")]
            public async Task AttackSegmentCommand(Direction direction)
            {
                var segment = TicTacZapController.GetUserSegmentSelection((await this.GetUserAsync()).UserId);
                if (!segment.HasValue)
                {
                    await ReplyAsync(Responses.SelectSegmentMessage);
                    return;
                }

                if (!await TicTacZapController.SegmentReadyForCombat(segment.Value))
                {
                    await ReplyAsync("This segment isn't ready for combat (needs to have offensive blocks and not already be in combat)");
                    return;
                }

                var defendingSegment = await UniverseController.GetAdjacentSegmentInDirection(segment.Value, direction);
                if (defendingSegment.HasValue)
                {
                    Direction opposite;
                    switch (direction)
                    {
                        case Direction.Up:
                            opposite = Direction.Down;
                            break;
                        case Direction.Down:
                            opposite = Direction.Up;
                            break;
                        case Direction.Left:
                            opposite = Direction.Right;
                            break;
                        case Direction.Right:
                            opposite = Direction.Left;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }
                    await ReplyAsync("Attacker:");
                    var attackMessage = await ReplyAsync("```Loading...```");
                    await ReplyAsync("Defender:");
                    var defenceMessage = await ReplyAsync("```Loading...```");
                    TicTacZapController.CurrentWars.Add((defendingSegment.Value, segment.Value, opposite, attackMessage));
                    TicTacZapController.CurrentWars.Add((segment.Value, defendingSegment.Value, direction, defenceMessage));
                    return;
                }

                await ReplyAsync("Good luck fighting... empty space? Try attacking something more interesting please");
            }

            private async Task<(int segmentId, (int x, int y)? coords)> NewSegment(int userId)
            {
                var user = Db.Users.First(u => u.UserId == userId);
                var segment = new Segment
                {
                    UnitsPerTick = 0,
                    EnergyPerTick = 0,
                    User = user
                };
                await Db.Segments.AddAsync(segment);
                await Db.SaveChangesAsync();
                await TicTacZapManagement.Segments.NewSegmentAsync(segment.SegmentId);
                await this.UpdateDbSegmentOutput(segment.SegmentId);
                var coords = await UniverseController.NewSegment(segment.SegmentId);
                return (segment.SegmentId, coords);
            }

            private async Task<(int x, int y)?> DeleteSegment(int segmentId)
            {
                var dbSegment = Db.Segments.First(s => s.SegmentId == segmentId);
                Db.Segments.Remove(dbSegment);
                await Db.SaveChangesAsync();
                await TicTacZapManagement.Segments.DeleteSegmentAsync(segmentId);
                return await UniverseController.DeleteSegment(segmentId);
            }
        }
    }
}