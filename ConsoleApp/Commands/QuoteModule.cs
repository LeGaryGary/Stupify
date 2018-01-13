using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.Client;
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
                db.Quotes.Add(new Quote()
                {
                    QuoteBody = quoteBody,
                    ServerUser = db.GetServerUser((long) Context.User.Id, (long) Context.Guild.Id)
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
        public async Task RandomQuote([Remainder] string quoteBody = null)
        {
            var sw = new Stopwatch();
            sw.Start();

            using (var db = new BotContext())
            {
                var quote = db
                    .Quotes
                    .Include(q => q.ServerUser.User)
                    .Include(q => q.ServerUser.Server)
                    .OrderBy(r => Guid.NewGuid())
                    .FirstOrDefault();
                if (quote == null)
                {
                    await ReplyAsync("No quotes were found, try !addquote <quote>");
                    return;
                }

                var message = quote.QuoteBody + " - " + db.UsernameFromServerUser(quote.ServerUser);
                sw.Stop();
                await ReplyAsync(message);
                if (Config.Debug) await ReplyAsync("Debug: ms to execute " + sw.ElapsedMilliseconds);
            }
        }
    }
}