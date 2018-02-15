using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;
using TicTacZap.Segment.Blocks;

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

        [Command("motherlode")]
        public async Task DebugMotherlode()
        {
            var user = await GetUserAsync();
            var balance = user.Balance;

            user.Balance += 1000000;

            await Db.SaveChangesAsync();
            await ReplyAsync($"You filthy cheater! Fine. I updated the balance. (balance: {user.Balance})");
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

            int id = await NewSegment(user);
            user.Balance -= price;
            
            await Db.SaveChangesAsync();
            await ReplyAsync($"You have purchased a segment! (id: {id})");
        }

        [Command("deletesegment")]
        public async Task DeleteSegmentCommand(int segmentId)
        {
            if (!await UserHasSegmentAsync(segmentId))
            {
                await ReplyAsync("You don't own a segment with this Id!");
            }

            await DeleteSegment(segmentId);
            await ReplyAsync("It's gone...");
        }

        [Command("addblock")]
        public async Task AddBlockCommand(int segmentId, int x, int y, string type)
        {
            await TicTacZapController.AddBlock(segmentId, x, y, (BlockType)Enum.Parse(typeof(BlockType), type));
            await UpdateDbSegmentOutput(segmentId);
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

        private async Task<int> NewSegment(User user)
        {
            var segment = new Segment
            {
                OutputPerTick = 0,
                UserId = user.UserId
            };
            await Db.Segments.AddAsync(segment);
            await Db.SaveChangesAsync();
            await TicTacZapController.AddSegment(segment.SegmentId);
            await UpdateDbSegmentOutput(segment.SegmentId);

            return segment.SegmentId;
        }

        private async Task UpdateDbSegmentOutput(int segmentId)
        {
            var segment = await Db.Segments.FirstAsync(s => s.SegmentId == segmentId);
            segment.OutputPerTick = TicTacZapController.GetSegmentOutput(segmentId);
            await Db.SaveChangesAsync();
        }

        private static decimal SegmentPrice(int segmentCount)
        {
            return Convert.ToDecimal(Math.Pow(2, segmentCount)-1) * 100;
        }
    }
}
