using System;
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
        [Command("balance")]
        public async Task ShowBalance()
        {
            using (var db = new BotContext())
            {
                var balance = await Balance(db);
                await ReplyAsync($"Your balance is: {balance}");
            }
        }

        [Command("buysegment")]
        public async Task BuySegment()
        {
            using (var db = new BotContext())
            {
                var user = await GetUser(db);
                var segments = await SegmentCount(db);

                var price = Convert.ToInt32(Math.Pow(2, segments)-1);
                if (price <= user.Balance)
                {
                    var segment = new Segment
                    {
                        OutputPerTick = 1,
                        UserId = user.UserId
                    };
                    db.Segments.Add(segment);
                    await db.SaveChangesAsync();

                    await TicTacZapController.AddSegment(segment.SegmentId);

                    user.Balance -= price;
                    await db.SaveChangesAsync();
                }
                else
                {
                    await ReplyAsync($"Come back when you have more money (you need {price} units to buy this)") ;
                }
            }
        }

        private async Task<int> SegmentCount(BotContext db)
        {
            var user = await GetUser(db);
            return await db.Segments.Where(s => s.UserId == user.UserId).CountAsync();
        }

        private async Task<decimal> Balance(BotContext db)
        {
            var user = await GetUser(db);
            return user.Balance;
        }

        private async Task<User> GetUser(BotContext db)
        {
            return await db.Users.FirstAsync(u => u.DiscordUserId == (long) Context.User.Id);
        }
    }
}
