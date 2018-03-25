using System.Threading.Tasks;
using Discord;
using Stupify.Data.Models;

namespace Stupify.Data.Repositories
{
    public interface IQuoteRepository
    {
        Task<Quote> RandomQuote();
        Task<Quote> RandomQuote(IGuild guild);
        Task<Quote> RandomQuote(IUser user);

        Task AddQuoteAsync(Quote quote, IGuild guild);
    }
}
