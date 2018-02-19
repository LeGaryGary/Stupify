using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap;

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
            {
                await ReplyAsync($"You don't have any segments, buy your first one: `{Config.CommandPrefix}Segment Buy`");
            }
            else
            {
                await ReplyAsync(renderSegmentList);
            }
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

            [Command("Buy")]
            public async Task BuySegment()
            {
                var user = await this.GetUserAsync();

                var price = SegmentPrice(await this.SegmentCountAsync());
                if (price > user.Balance)
                {
                    await ReplyAsync(Responses.NotEnoughUnits(price));
                    return;
                }

                var tuple = await NewSegment(user);
                user.Balance -= price;

                await Db.SaveChangesAsync();
                await ReplyAsync($"You have purchased a segment!\r\nId: {tuple.segmentId}\r\nCoordinates: {tuple.coords.x}, {tuple.coords.y}");
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
                {
                    if (type.Value > 0)
                    {
                        await Inventories.AddToInventoryAsync(type.Key, type.Value, user.UserId);
                    }
                }

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

                var tuple = await DeleteSegment(segmentId);
                await ReplyAsync($"It's gone...\r\nId: {segmentId}\r\nCoordinates: {tuple.x+1}, {tuple.y+1}");
            }

            private async Task<(int segmentId,(int x,int y) coords)> NewSegment(User user)
            {
                var segment = new Segment
                {
                    UnitsPerTick = 0,
                    EnergyPerTick = 0,
                    Energy = 0,
                    UserId = user.UserId
                };
                await Db.Segments.AddAsync(segment);
                await Db.SaveChangesAsync();
                await TicTacZapManagement.Segments.NewSegmentAsync(segment.SegmentId);
                await this.UpdateDbSegmentOutput(segment.SegmentId);
                var coords = await UniverseController.NewSegment(segment.SegmentId);
                return (segment.SegmentId, coords);
            }

            private async Task<(int x, int y)> DeleteSegment(int segmentId)
            {
                var dbSegment = Db.Segments.First(s => s.SegmentId == segmentId);
                Db.Segments.Remove(dbSegment);
                await Db.SaveChangesAsync();
                await TicTacZapManagement.Segments.DeleteSegmentAsync(segmentId);
                return await UniverseController.DeleteSegment(segmentId);
            }
        }
        
        private static string RenderSegmentList(IEnumerable<Segment> segments)
        {
            var str = string.Empty;
            foreach (var segment in segments)
            {
                str += $"Segment Id: {segment.SegmentId} Output: {segment.UnitsPerTick}" + Environment.NewLine;
            }

            return str;
        }

        private static decimal SegmentPrice(int segmentCount)
        {
            return Convert.ToDecimal(Math.Pow(2, segmentCount)-1) * 100;
        }
    }
}
