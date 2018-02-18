using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;
using TicTacZap;

namespace StupifyConsoleApp.Commands.TicTacZap
{
    public class SegmentModule:ModuleBase<SocketCommandContext>
    {
        private BotContext Db { get; } = new BotContext();

        [Command("segment")]
        public async Task ShowSegment(int segmentId)
        {
            if (await CommonFunctions.UserHasSegmentAsync(Db, Context, segmentId))
            {
                await TicTacZapController.SetUserSegmentSelection((await CommonFunctions.GetUserAsync(Db, Context)).UserId, segmentId, Db);
                await ReplyAsync($"```{await TicTacZapController.RenderSegmentAsync(segmentId, Db)}```");
                return;
            }
            await ReplyAsync(Responses.SegmentOwnershipProblem);
        }

        [Command("segments")]
        public async Task ListSegments()
        {
            var segments = await CommonFunctions.GetSegments(Db, Context);
            var renderSegmentList = RenderSegmentList(segments);
            if (renderSegmentList == String.Empty)
            {
                await ReplyAsync($"You dont have any segments, buy your first one: `{Config.CommandPrefix}buysegment`");
            }
            else
            {
                await ReplyAsync(renderSegmentList);
            }
        }

        [Command("buysegment")]
        public async Task BuySegment()
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);

            var price = SegmentPrice(await SegmentCountAsync());
            if (price > user.Balance)
            {
                await ReplyAsync(CommonFunctions.NotEnoughUnits(price));
                return;
            }

            int id = await NewSegment(user);
            user.Balance -= price;
            
            await Db.SaveChangesAsync();
            await ReplyAsync($"You have purchased a segment! (id: {id})");
        }

        [Command("resetsegment")]
        public async Task ResetSegment(int segmentId)
        {
            if (!await CommonFunctions.UserHasSegmentAsync(Db, Context, segmentId))
            {
                await ReplyAsync(Responses.SegmentOwnershipProblem);
                return;
            }

            var user = await CommonFunctions.GetUserAsync(Db, Context);
            var blocks = await Segments.ResetSegmentAsync(segmentId);

            foreach (var type in blocks)
            {
                if (type.Value > 0)
                {
                    await Inventories.AddToInventoryAsync(type.Key, type.Value, user.UserId);
                }
            }

            await Db.SaveChangesAsync();
            await UpdateDbSegmentOutput(segmentId);
            await ReplyAsync($"segment {segmentId} was reset!");
        }

        [Command("deletesegment")]
        public async Task DeleteSegmentCommand(int segmentId)
        {
            if (!await CommonFunctions.UserHasSegmentAsync(Db, Context, segmentId))
            {
                await ReplyAsync(Responses.SegmentOwnershipProblem);
                return;
            }

            await DeleteSegment(segmentId);
            await ReplyAsync("It's gone...");
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

        private async Task<int> NewSegment(User user)
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
            await Segments.NewSegmentAsync(segment.SegmentId);
            await UpdateDbSegmentOutput(segment.SegmentId);

            return segment.SegmentId;
        }

        protected async Task UpdateDbSegmentOutput(int segmentId)
        {
            var dbSegment = await Db.Segments.FirstAsync(s => s.SegmentId == segmentId);
            var segmentOutput = await Segments.GetOutput(segmentId);

            dbSegment.UnitsPerTick = segmentOutput[Resource.Unit];
            dbSegment.EnergyPerTick = segmentOutput[Resource.Energy];

            await Db.SaveChangesAsync();
        }

        private static decimal SegmentPrice(int segmentCount)
        {
            return Convert.ToDecimal(Math.Pow(2, segmentCount)-1) * 100;
        }

        private async Task DeleteSegment(int segmentId)
        {
            var dbSegment = Db.Segments.First(s => s.SegmentId == segmentId);
            Db.Segments.Remove(dbSegment);
            await Db.SaveChangesAsync();

            await Segments.DeleteSegmentAsync(segmentId);
        }
        
        private async Task<int> SegmentCountAsync()
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);
            return await Db.Segments.Where(s => s.UserId == user.UserId).CountAsync();
        }
    }
}
