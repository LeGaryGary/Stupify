using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZap;
using TicTacZap.Segment.Blocks;

namespace StupifyConsoleApp.Commands.TicTacZap
{
    public class BlockModule:SegmentModule
    {
        private BotContext Db { get; } = new BotContext();

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
    }
}
