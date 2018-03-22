using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stupify.Data;
using Stupify.Data.Models;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp.Commands.Modules
{
    public class Quotes : ModuleBase<CommandContext>
    {
        private readonly ILogger<Quote> _logger;
        private readonly IQuoteRepository _quoteRepository;

        public Quotes(ILogger<Quote> logger, IQuoteRepository quoteRepository)
        {
            _logger = logger;
            _quoteRepository = quoteRepository;
        }

        [Command("AddQuote")]
        public async Task AddQuoteAsync(string quoteBody)
        {
            await _quoteRepository.AddQuoteAsync(new Quote
            {
                Author = Context.User,
                Content = quoteBody
            });
            _logger.LogInformation("Added quote from {User} in {Guild}", Context.User.Username, Context.Guild.Name);
            await ReplyAsync("Done!");
        }

        [Command("RandomQuote")]
        public async Task RandomQuoteAsync()
        {
            var quote = await _quoteRepository.RandomQuote(Context.Guild);

            if (quote == null)
            {
                await ReplyAsync("No quotes were found, try !addquote <quote>");
                return;
            }

            var message = $"{quote.Content} - {quote.Author.Username}";
            await ReplyAsync(message);
        }
    }
}