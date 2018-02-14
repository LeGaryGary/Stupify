using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;

namespace StupifyConsoleApp.Commands
{
    public class TicTacZapModule : ModuleBase<SocketCommandContext>
    {
        private BotContext Db { get; } = new BotContext();

        [Command("balance")]
        public async Task ShowBalance()
        {
            var balance = await Balance();
            await ReplyAsync($"Your balance is: {balance}");
        }

        [Command("segment")]
        public async Task ShowSegment(int segmentId)
        {
            await ReplyAsync(TicTacZapController.RenderSegment(segmentId));
        }

        [Command("segments")]
        public async Task ListSegments()
        {
            var segments = await GetSegments();
            var renderSegmentList = RenderSegmentList(segments);
            if (renderSegmentList == string.Empty)
            {
                await ReplyAsync("You dont have any segments, buy your first one: `!s buysegment`");
            }
            else
            {
                await ReplyAsync(renderSegmentList);
            }
        }

        [Command("buysegment")]
        public async Task BuySegment()
        {
            var user = await GetUserAsync();

            var price = SegmentPrice(await SegmentCountAsync());
            if (price > user.Balance)
            {
                await ReplyAsync($"Come back when you have more money (you need {price} units to buy this)");
                return;
            }

            await NewSegment(user);
            user.Balance -= price;
            
            await Db.SaveChangesAsync();
            await ReplyAsync("You have purchased a segment!");
        }

        [Command("deletesegment")]
        public async Task DeleteSegmentCommand(int segmentId)
        {
            if (!await UserHasSegmentAsync(segmentId))
            {
                await ReplyAsync("You don't own a segment with this Id!");
            }

            await DeleteSegment(segmentId);
            await ReplyAsync("Its gone...");
        }

        private async Task DeleteSegment(int segmentId)
        {
            var dbSegment = Db.Segments.First(s => s.SegmentId == segmentId);
            Db.Segments.Remove(dbSegment);
            await Db.SaveChangesAsync();

            TicTacZapController.DeleteSegment(segmentId);
        }

        private async Task<bool> UserHasSegmentAsync(int segmentId)
        {
            return (await GetSegments()).Select(s => s.SegmentId).Contains(segmentId);
        }

        private static string RenderSegmentList(IEnumerable<Segment> segments)
        {
            var str = string.Empty;
            foreach (var segment in segments)
            {
                str += $"Segment Id: {segment.SegmentId} Output: {segment.OutputPerTick}" + Environment.NewLine;
            }

            return str;
        }

        private async Task<IEnumerable<Segment>> GetSegments()
        {
            var user = await GetUserAsync();
            return Db.Segments.Where(s => s.UserId == user.UserId);
        }

        private async Task<int> SegmentCountAsync()
        {
            var user = await GetUserAsync();
            return await Db.Segments.Where(s => s.UserId == user.UserId).CountAsync();
        }

        private async Task<decimal> Balance()
        {
            var user = await GetUserAsync();
            return user.Balance;
        }

        private async Task<User> GetUserAsync()
        {
            return await Db.Users.FirstAsync(u => u.DiscordUserId == (long) Context.User.Id);
        }

        private async Task NewSegment(User user)
        {
            var segment = new Segment
            {
                OutputPerTick = 1,
                UserId = user.UserId
            };
            await Db.Segments.AddAsync(segment);
            await TicTacZapController.AddSegment(segment.SegmentId);
            
        }

        private static decimal SegmentPrice(int segmentCount)
        {
            return Convert.ToDecimal(Math.Pow(2, segmentCount)-1) * 100;
        }
    }
}
