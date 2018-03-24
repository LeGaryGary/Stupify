using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.FileSystem;
using Stupify.Data.SQL;
using Stupify.Data.SQL.Models;
using Segment = TicTacZap.Segment;

namespace Stupify.Data.Repositories
{
    internal class TemplateRepository : ITemplateRepository
    {
        private readonly SegmentTemplates _segmentTemplates;
        private readonly BotContext _botContext;
        private readonly IUserRepository _userRepository;

        public TemplateRepository(SegmentTemplates segmentTemplates, BotContext botContext, IUserRepository userRepository)
        {
            _segmentTemplates = segmentTemplates;
            _botContext = botContext;
            _userRepository = userRepository;
        }

        public async Task<int> NewTemplateAsync(Segment segment, IUser user)
        {
            var userId = await _userRepository.GetUserId(user);
            var dbUser = await _botContext.Users.FirstAsync(u => u.UserId == userId);
            var dbTemplate = _botContext.SegmentTemplates.Add(new SegmentTemplate
            {
                User = dbUser
            }).Entity;
            await _botContext.SaveChangesAsync();

            await _segmentTemplates.SaveAsync(dbTemplate.SegmentTemplateId, segment);

            return dbTemplate.SegmentTemplateId;
        }

        public async Task<bool> UserHasTemplateAsync(IUser user, int templateId)
        {
            var userId = await _userRepository.GetUserId(user);
            return await _botContext.SegmentTemplates.AnyAsync(st =>
                st.SegmentTemplateId == templateId && st.User.UserId == userId);
        }

        public async Task<Segment> GetTemplateAsync(int templateId)
        {
            return await _segmentTemplates.GetAsync(templateId);
        }

        public async Task<IEnumerable<(int id, string name)>> GetTemplatesAsync(IUser user)
        {
            var userId = await _userRepository.GetUserId(user);
            var templates = await _botContext.SegmentTemplates.Where(st => st.User.UserId == userId).ToArrayAsync();

            return templates.Select(template => (template.SegmentTemplateId, template.Name));
        }
    }
}
