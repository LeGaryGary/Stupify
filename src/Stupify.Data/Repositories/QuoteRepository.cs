using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.Models;
using Stupify.Data.SQL;

namespace Stupify.Data.Repositories
{
    internal class QuoteRepository : IQuoteRepository
    {
        private readonly BotContext _botContext;
        private readonly IDiscordClient _discordClient;

        public QuoteRepository(BotContext botContext, IDiscordClient discordClient)
        {
            _botContext = botContext;
            _discordClient = discordClient;
        }

        public async Task<Quote> RandomQuote()
        {
            var dbQuote = await _botContext.Quotes
                .Include(q => q.ServerUser.User)
                .Include(q => q.ServerUser.Server)
                .OrderBy(r => Guid.NewGuid())
                .FirstOrDefaultAsync();
            if (dbQuote == null) return null;
            var server = await _discordClient.GetGuildAsync((ulong) dbQuote.ServerUser.Server.DiscordGuildId);
            var user = await server.GetUserAsync((ulong) dbQuote.ServerUser.User.DiscordUserId);
            return new Quote
            {
                Author = user,
                Content = dbQuote.QuoteBody
            };
        }

        public async Task<Quote> RandomQuote(IGuild guild)
        {
            var dbQuote = await _botContext.Quotes
                .Include(q => q.ServerUser.User)
                .Include(q => q.ServerUser.Server)
                .OrderBy(r => Guid.NewGuid())
                .Where(q => q.ServerUser.Server.DiscordGuildId == (long)guild.Id)
                .FirstOrDefaultAsync();
            if (dbQuote == null) return null;

            var server = await _discordClient.GetGuildAsync((ulong) dbQuote.ServerUser.Server.DiscordGuildId);
            var user = await server.GetUserAsync((ulong) dbQuote.ServerUser.User.DiscordUserId);
            return new Quote
            {
                Author = user,
                Content = dbQuote.QuoteBody
            };
        }

        public async Task<Quote> RandomQuote(IUser discordUser)
        {
            var dbQuote = await _botContext.Quotes
                .Include(q => q.ServerUser.User)
                .Include(q => q.ServerUser.Server)
                .OrderBy(r => Guid.NewGuid())
                .Where(q => q.ServerUser.User.DiscordUserId == (long)discordUser.Id)
                .FirstOrDefaultAsync();
            if (dbQuote == null) return null;

            var server = await _discordClient.GetGuildAsync((ulong) dbQuote.ServerUser.Server.DiscordGuildId);
            var user = await server.GetUserAsync((ulong) dbQuote.ServerUser.User.DiscordUserId);
            return new Quote
            {
                Author = user,
                Content = dbQuote.QuoteBody
            };
        }

        public async Task AddQuoteAsync(Quote quote, IGuild guild)
        {
            var serverUser = await _botContext.ServerUsers.FirstOrDefaultAsync(
                su => su.User.DiscordUserId == (long) quote.Author.Id && su.Server.DiscordGuildId == (long) guild.Id);

            _botContext.Quotes.Add(new SQL.Models.Quote
            {
                ServerUser = serverUser,
                QuoteBody = quote.Content
            });
            await _botContext.SaveChangesAsync();
        }
    }
}
