using System.Threading.Tasks;
using Discord;
using Stupify.Data.Models;

namespace Stupify.Data
{
    public interface IQuoteRepository
    {
        Task<Quote> RandomQuote();
        Task<Quote> RandomQuote(IGuild guild);
        Task<Quote> RandomQuote(IUser user);

        Task AddQuoteAsync(Quote quote);

        Task<bool> DeleteQuote(Quote quote);
    }
}
