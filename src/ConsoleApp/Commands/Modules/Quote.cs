using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Commands.Modules
{
    public class Quote : StupifyModuleBase
    {
        private readonly ILogger<Quote> _logger;

        public Quote(ILogger<Quote> logger, BotContext db): base(db)
        {
            _logger = logger;
        }

        [Command("AddQuote")]
        public async Task AddQuoteAsync([Remainder] string quoteBody)
        {
            await Db.Quotes.AddAsync(new DataModels.Quote
            {
                QuoteBody = quoteBody,
                ServerUser = await Db.GetServerUserAsync((long) Context.User.Id, (long) Context.Guild.Id)
            });
            await Db.SaveChangesAsync();
            _logger.LogInformation("The following quote has been added!: {quoteBody}", quoteBody);
            await ReplyAsync("Done!");
        }

        [Command("RandomQuote")]
        public async Task RandomQuoteAsync()
        {
            var quote = await Db
                .Quotes
                .Include(q => q.ServerUser.User)
                .Include(q => q.ServerUser.Server)
                .Where(q => (ulong) q.ServerUser.Server.DiscordGuildId == Context.Guild.Id)
                .OrderBy(r => Guid.NewGuid())
                .FirstOrDefaultAsync();
            if (quote == null)
            {
                await ReplyAsync("No quotes were found, try !addquote <quote>");
                return;
            }

            var message = quote.QuoteBody + " - " + Context.Client.UsernameFromServerUser(quote.ServerUser);
            await ReplyAsync(message);
        }
    }
}