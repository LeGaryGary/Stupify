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
        private const string SegmentOwnershipProblemString = "You don't own a segment with this Id!";
        private readonly string _selectSegmentMessage = $"Please select a segment with {Config.CommandPrefix}segment [segmentId]";
        private readonly string _buyItemAdvisory = $"Please buy the item you are trying to use! `{Config.CommandPrefix}shop` and `{Config.CommandPrefix}buy [type] [quantity]`";

        private BotContext Db { get; } = new BotContext();

        [Command("balance")]
        public async Task ShowBalance()
        {
            var balance = await Balance();
            await ReplyAsync($"Your balance is: {balance}");
        }


        // debug
        [Command("motherlode")]
        public async Task DebugMotherlode()
        {
            var user = await GetUserAsync();
            var balance = user.Balance;

            user.Balance += 1000000;

            await Db.SaveChangesAsync();
            await ReplyAsync($"You filthy cheater! Fine. I updated the balance. (balance: {user.Balance})");
        }
        // AI
        [Command("solve")]
        public async Task DebugSolve(int segmentId)
        {
            var user = await GetUserAsync();
            var segment = Db.Segments.First(s => s.SegmentId == segmentId);
            if(segment == null)
            {
                await ReplyAsync("invalid segment ID");
                return;
            }

        }

        [Command("solve")]
        public async Task DebugSolve()
        {
            User user = await GetUserAsync();
            var id = TicTacZapController.GetUserSelection(user.UserId);

            if (id != null)
            {
                await DebugSolve((int)id);
            }
            else
            {
                await ReplyAsync(_selectSegmentMessage);
            }
        }

        [Command("inventory")]
        public async Task ShowInventory()
        {
            var userId = (await GetUserAsync()).UserId;
            var message = await TicTacZapController.RenderInventory(userId);
            if (message == string.Empty)
            {
                await ReplyAsync("Your inventory is empty");
                return;
            }
            await ReplyAsync(message);
        }

        [Command("shop")]
        public async Task ShowShopInventory()
        {
            var message = TicTacZapController.Shop.TextRender();
            await ReplyAsync(message);
        }

        [Command("buy")]
        public async Task BuyFromShop(string blockString, int quantity)
        {
            var block = Enum.Parse<BlockType>(blockString);
            var total = TicTacZapController.Shop.GetTotal(block, quantity);
            if (await RemoveBalanceAsync(total))
            {
                await TicTacZapController.AddToInventoryAsync(block, quantity, (await GetUserAsync()).UserId);
                await ShowInventory();
                return;
            }

            await NotEnoughUnitsReplyAsync(total);
        }

        [Command("segment")]
        public async Task ShowSegment(int segmentId)
        {
            if (await UserHasSegmentAsync(segmentId))
            {
                TicTacZapController.SetUserSegmentSelection((await GetUserAsync()).UserId, segmentId);
                await ReplyAsync("```"+TicTacZapController.RenderSegment(segmentId)+"```");
                return;
            }
            await ReplyAsync(SegmentOwnershipProblemString);
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
                await NotEnoughUnitsReplyAsync(price);
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
                await ReplyAsync(SegmentOwnershipProblemString);
                return;
            }

            await DeleteSegment(segmentId);
            await ReplyAsync("It's gone...");
        }

        [Command("addblock")]
        public async Task AddBlockCommand(int segmentId, int x, int y, string type)
        {

            await TicTacZapController.AddBlock(segmentId, x - 1, y - 1, (BlockType)Enum.Parse(typeof(BlockType), type));
            await UpdateDbSegmentOutput(segmentId);
            await ShowSegment(segmentId);

            var blockType = Enum.Parse<BlockType>(type);
            if (await TicTacZapController.RemoveFromInventory(blockType, 1, (await GetUserAsync()).UserId))
            {
                await TicTacZapController.AddBlock(segmentId, x-1, y-1, blockType);
                await UpdateDbSegmentOutput(segmentId);
                await ShowSegment(segmentId);
                return;
            }

            await ReplyAsync(_buyItemAdvisory);

        }

        [Command("addblock")]
        public async Task AddBlockCommand(int x, int y, string type)
        {
            var segmentSelectionId = TicTacZapController.GetUserSelection((await GetUserAsync()).UserId);
            if (segmentSelectionId != null )
            {
                await AddBlockCommand((int)segmentSelectionId, x, y, type);
                return;
            }

            await ReplyAsync(_selectSegmentMessage);
        }

        [Command("removeblock")]
        public async Task RemoveBlockCommand(int segmentId, int x, int y)
        {
            if (await UserHasSegmentAsync(segmentId))
            {
                var blockType = await TicTacZapController.DeleteBlockAsync(segmentId, x-1, y-1);

                if (blockType != null) await TicTacZapController.AddToInventoryAsync(blockType.Value, 1, (await GetUserAsync()).UserId);
                await ShowSegment(segmentId);
                return;
            }

            await ReplyAsync(SegmentOwnershipProblemString);
        }

        [Command("removeblock")]
        public async Task RemoveBlockCommand(int x, int y)
        {
            var segmentSelectionId = TicTacZapController.GetUserSelection((await GetUserAsync()).UserId);
            if (segmentSelectionId != null )
            {
                await RemoveBlockCommand((int)segmentSelectionId, x, y);
                return;
            }

            await ReplyAsync(_selectSegmentMessage);
        }

        private async Task NotEnoughUnitsReplyAsync(decimal price)
        {
            await ReplyAsync($"Come back when you have more money (you need {price} units to buy this)");
        }

        private async Task<bool> RemoveBalanceAsync(decimal units)
        {
            var user = await GetUserAsync();
            if (units > user.Balance) return false;
            user.Balance -= units;
            await Db.SaveChangesAsync();
            return true;
        }

        private async Task DeleteSegment(int segmentId)
        {
            var dbSegment = Db.Segments.First(s => s.SegmentId == segmentId);
            Db.Segments.Remove(dbSegment);
            await Db.SaveChangesAsync();

            await TicTacZapController.DeleteSegmentAsync(segmentId);
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
