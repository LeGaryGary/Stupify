using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp.DataModels;
using Discord.Commands;

namespace ConsoleApp.Commands
{
    public class QuoteModule : ModuleBase<SocketCommandContext>
    {
        [Command("addquote")]
        public async Task AddQuote([RemainderAttribute] string quoteBody)
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
