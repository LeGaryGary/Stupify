using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using TicTacZap;

namespace Stupify.Data.Repositories
{
    public interface ITemplateRepository
    {
        Task<int> NewTemplateAsync(Segment segment, IUser user);
        Task<bool> UserHasTemplateAsync(IUser user, int templateId);
        Task<Segment> GetTemplateAsync(int templateId);
        Task<IEnumerable<(int id, string name)>> GetTemplatesAsync(IUser user);
    }
}