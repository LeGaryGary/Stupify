using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Commands
{
    public class QuoteModule : ModuleBase<SocketCommandContext>
    {
        [Command("addquote")]
        public async Task AddQuote([Remainder] string quoteBody)
        {
            using (var db = new BotContext())
            {
                /*
                var serverUser = db.ServerUsers.FirstOrDefault(
                    u => u.User.UserId == (long) Context.User.Id &&
                         u.Server.ServerId == (long) Context.Guild.Id);
                if (serverUser != null)
                {
                    var serverUserId = serverUser.ServerUserId;
                }
                */
                db.Quotes.Add(new Quote()
                    {
                        QuoteBody = quoteBody,
                        ServerUser = db.GetServerUser((long)Context.User.Id, (long)Context.Guild.Id)
                });
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                await ReplyAsync("Done!");
            }
        }

        [Command("randomquote")]
        public async Task RandomQuote()
        {
            using (var db = new BotContext())
            {
                var quote = db.Quotes.OrderBy(r => Guid.NewGuid()).FirstOrDefault();
                if (quote == null)
                {
                    await ReplyAsync("No quotes were found, try !addquote <quote>");
                    return;
                }
                await ReplyAsync(quote.QuoteBody);
            }
        }
    }
}
