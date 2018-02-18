using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;
using TicTacZap;
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

        [Command("inventory")]
        public async Task ShowInventory()
        {
            var userId = (await CommonFunctions.GetUserAsync(Db, Context)).UserId;
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
                await Inventories.AddToInventoryAsync(block, quantity, (await CommonFunctions.GetUserAsync(Db, Context)).UserId);
                await ShowInventory();
                return;
            }

            await NotEnoughUnitsReplyAsync(total);
        }

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
            var user = await CommonFunctions.GetUserAsync(Db, Context);

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

        [Command("addblock")]
        public async Task AddBlockCommand(int segmentId, int x, int y, string type)
        {
            var blockType = Enum.Parse<BlockType>(type);
            if (await Inventories.RemoveFromInventoryAsync(blockType, 1, (await CommonFunctions.GetUserAsync(Db, Context)).UserId))
            {
                await Segments.AddBlockAsync(segmentId, x-1, y-1, blockType);
                await UpdateDbSegmentOutput(segmentId);
                await ShowSegment(segmentId);
                return;
            }

            await ReplyAsync(Responses.ShopAdvisoryMessage);
        }

        [Command("addblock")]
        public async Task AddBlockCommand(int x, int y, string type)
        {
            var segmentSelectionId = TicTacZapController.GetUserSelection((await CommonFunctions.GetUserAsync(Db, Context)).UserId);
            if (segmentSelectionId != null )
            {
                await AddBlockCommand((int)segmentSelectionId, x, y, type);
                return;
            }

            await ReplyAsync(Responses.SelectSegmentMessage);
        }

        [Command("removeblock")]
        public async Task RemoveBlockCommand(int segmentId, int x, int y)
        {
            if (await CommonFunctions.UserHasSegmentAsync(Db, Context, segmentId))
            {
                var blockType = await Segments.DeleteBlockAsync(segmentId, x-1, y-1);

                if (blockType != null) await Inventories.AddToInventoryAsync(blockType.Value, 1, (await CommonFunctions.GetUserAsync(Db, Context)).UserId);
                await ShowSegment(segmentId);
                return;
            }

            await ReplyAsync(Responses.SegmentOwnershipProblem);
        }

        [Command("removeblock")]
        public async Task RemoveBlockCommand(int x, int y)
        {
            var segmentSelectionId = TicTacZapController.GetUserSelection((await CommonFunctions.GetUserAsync(Db, Context)).UserId);
            if (segmentSelectionId != null )
            {
                await RemoveBlockCommand((int)segmentSelectionId, x, y);
                return;
            }

            await ReplyAsync(Responses.SelectSegmentMessage);
        }

        private async Task NotEnoughUnitsReplyAsync(decimal price)
        {
            await ReplyAsync($"Come back when you have more money (you need {price} units to buy this)");
        }

        private async Task<bool> RemoveBalanceAsync(decimal units)
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);
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

            await Segments.DeleteSegmentAsync(segmentId);
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

        private async Task<int> SegmentCountAsync()
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);
            return await Db.Segments.Where(s => s.UserId == user.UserId).CountAsync();
        }

        private async Task<decimal> Balance()
        {
            var user = await CommonFunctions.GetUserAsync(Db, Context);
            return user.Balance;
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

        private async Task UpdateDbSegmentOutput(int segmentId)
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
    }
}
