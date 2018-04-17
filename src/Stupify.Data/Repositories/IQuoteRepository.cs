using System.Threading.Tasks;
using Discord;
using Stupify.Data.Models;

namespace Stupify.Data.Repositories
{
    public interface IQuoteRepository
    {
        Task<Quote> RandomQuoteAsync();
        Task<Quote> RandomQuoteAsync(IGuild guild);
        Task<Quote> RandomQuoteAsync(IUser user);

        Task AddQuoteAsync(Quote quote, IGuild guild);
    }
}
