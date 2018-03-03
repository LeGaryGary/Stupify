using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap;

namespace StupifyConsoleApp.Commands
{
    public static class CommonFunctions
    {
        public static async Task<User> GetUserAsync(this StupifyModuleBase moduleBase)
        {
            return await moduleBase.Db.Users.FirstAsync(u => u.DiscordUserId == (long) moduleBase.Context.User.Id);
        }

        public static async Task<IEnumerable<DataModels.Segment>> GetSegments(this StupifyModuleBase moduleBase)
        {
            var user = await GetUserAsync(moduleBase);
            return moduleBase.Db.Segments.Where(s => s.User.UserId == user.UserId);
        }

        public static async Task<bool> UserHasSegmentAsync(this StupifyModuleBase moduleBase, int segmentId)
        {
            return (await GetSegments(moduleBase)).Select(s => s.SegmentId).Contains(segmentId);
        }

        public static async Task<int> SegmentCountAsync(this StupifyModuleBase moduleBase)
        {
            var user = await moduleBase.GetUserAsync();
            return await moduleBase.Db.Segments.Where(s => s.User.UserId == user.UserId).CountAsync();
        }

        public static async Task ShowSegmentAsync(this StupifyModuleBase moduleBase, int segmentId)
        {
            await TicTacZapController.SetUserSegmentSelection(
                (await moduleBase.GetUserAsync()).UserId, segmentId, moduleBase.Db);
            await moduleBase.Context.Channel.SendMessageAsync(
                $"```{await TicTacZapController.RenderSegmentAsync(segmentId, moduleBase.Db)}```");
        }

        public static async Task ShowTemplateAsync(this StupifyModuleBase moduleBase, int templateId)
        {
            await TicTacZapController.SetUserTemplateSelection(
                (await moduleBase.GetUserAsync()).UserId,
                templateId,
                moduleBase.Db);

            await moduleBase.Context.Channel.SendMessageAsync(
                $"```{(await SegmentTemplates.GetAsync(templateId)).TextRender()}```");
        }

        public static async Task UpdateDbSegmentOutput(this StupifyModuleBase moduleBase, int segmentId)
        {
            var dbSegment = await moduleBase.Db.Segments.FirstAsync(s => s.SegmentId == segmentId);
            var segmentOutput = await Segments.GetOutput(segmentId);

            dbSegment.UnitsPerTick = segmentOutput[Resource.Unit];
            dbSegment.EnergyPerTick = segmentOutput[Resource.Energy];

            await moduleBase.Db.SaveChangesAsync();
        }

        public static async Task ClearSegmentToInventory(this StupifyModuleBase moduleBase, int segmentId, int userId)
        {
            var blocks = await Segments.ResetSegmentAsync(segmentId);
            foreach (var pair in blocks)
            {
                if (pair.Value > 0)
                {
                    await Inventories.AddToInventoryAsync(pair.Key, pair.Value, userId);
                }
            }
        }
    }
}