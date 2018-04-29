using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Stupify.Data.FileSystem;
using Stupify.Data.SQL;
using TicTacZap;
using TicTacZap.Blocks;

namespace Stupify.Data.Repositories
{
    internal class SegmentRepository : ISegmentRepository
    {
        private readonly BotContext _botContext;
        private readonly FileSegments _segments;
        private readonly IDiscordClient _client;
        private readonly IUserRepository _userRepository;
        private readonly IUniverseRepository _universeRepository;

        public SegmentRepository(BotContext botContext, FileSegments segments, IDiscordClient client, IUserRepository userRepository, IUniverseRepository universeRepository)
        {
            _botContext = botContext;
            _segments = segments;
            _client = client;
            _userRepository = userRepository;
            _universeRepository = universeRepository;
        }

        public async Task<(int segmentId, (int x, int y)? coords)> NewSegmentAsync(IUser user)
        {
            var userId = await _userRepository.GetUserIdAsync(user).ConfigureAwait(false);

            var dbUser = await _botContext.Users.FirstAsync(u => u.UserId == userId).ConfigureAwait(false);
            var dbSegment = new SQL.Models.Segment
            {
                User = dbUser,
                EnergyPerTick = 0,
                UnitsPerTick = 0
            };
            _botContext.Segments.Add(dbSegment);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            await _segments.NewSegmentAsync(dbSegment.SegmentId).ConfigureAwait(false);
            return (dbSegment.SegmentId, await _universeRepository.NewSegmentAsync(dbSegment.SegmentId).ConfigureAwait(false));
        }

        public Task<Segment> GetSegmentAsync(int segmentId)
        {
            return _segments.GetAsync(segmentId);
        }

        public async Task SetSegmentAsync(int segmentId, Segment segment)
        {
            await _segments.SetAsync(segmentId, segment).ConfigureAwait(false);

            var dBSegment = await _botContext.Segments.FirstAsync(s => s.SegmentId == segmentId).ConfigureAwait(false);
            dBSegment.EnergyPerTick = segment.ResourcePerTick(Resource.Energy);
            dBSegment.UnitsPerTick = segment.ResourcePerTick(Resource.Unit);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<(int x, int y)?> DeleteSegmentAsync(int segmentId)
        {
            var dbSegment = await _botContext.Segments.FirstAsync(s => s.SegmentId == segmentId).ConfigureAwait(false);
            _botContext.Segments.Remove(dbSegment);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            await Task.Run(() => _segments.DeleteSegment(segmentId)).ConfigureAwait(false);
            return await _universeRepository.DeleteSegmentAsync(segmentId).ConfigureAwait(false);
        }

        public Task<Dictionary<BlockType, int>> ResetSegmentAsync(int segmentId)
        {
            return _segments.ResetSegmentAsync(segmentId);
        }

        public async Task<bool> AddBlockAsync(int segmentId, int x, int y, BlockType blockType)
        {
            var segment = await _segments.GetAsync(segmentId).ConfigureAwait(false);

            var addBlockResult = segment.AddBlock(x, y, blockType);

            if (!addBlockResult) return false;

            var dBSegment = await _botContext.Segments.FirstAsync(s => s.SegmentId == segmentId).ConfigureAwait(false);
            dBSegment.EnergyPerTick = segment.ResourcePerTick(Resource.Energy);
            dBSegment.UnitsPerTick = segment.ResourcePerTick(Resource.Unit);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            await _segments.SetAsync(segmentId, segment).ConfigureAwait(false);

            return true;
        }

        public async Task<BlockType?> DeleteBlockAsync(int segmentId, int x, int y)
        {
            var segment = await _segments.GetAsync(segmentId).ConfigureAwait(false);
            var deleteBlockResult = segment.DeleteBlock(x, y);

            if (!deleteBlockResult.HasValue) return null;

            await _segments.SetAsync(segmentId, segment).ConfigureAwait(false);

            var dBSegment = await _botContext.Segments.FirstAsync(s => s.SegmentId == segmentId).ConfigureAwait(false);
            dBSegment.EnergyPerTick = segment.ResourcePerTick(Resource.Energy);
            dBSegment.UnitsPerTick = segment.ResourcePerTick(Resource.Unit);
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            return deleteBlockResult;
        }

        public async Task<Dictionary<Resource, decimal>> GetOutputAsync(int segmentId)
        {
            return (await _segments.GetAsync(segmentId).ConfigureAwait(false)).ResourcePerTick();
        }

        public Task<bool> ExistsAsync(int segmentId)
        {
            return _botContext.Segments.AnyAsync(s => s.SegmentId == segmentId);
        }

        public async Task<int> SegmentCountAsync(IUser user)
        {
            var userId = await _userRepository.GetUserIdAsync(user).ConfigureAwait(false);
            return await _botContext.Segments.CountAsync(s => s.User.UserId == userId).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Segment>> GetSegmentsAsync(IUser user)
        {
            var userId = await _userRepository.GetUserIdAsync(user).ConfigureAwait(false);

            var segments = new List<Segment>();
            foreach (var segment in await _botContext.Segments.Where(s => s.User.UserId == userId).ToArrayAsync().ConfigureAwait(false))
            {
                segments.Add(await GetSegmentAsync(segment.SegmentId).ConfigureAwait(false));
            }

            return segments;
        }

        public async Task<IEnumerable<int>> GetSegmentIdsAsync(IUser user)
        {
            var userId = await _userRepository.GetUserIdAsync(user).ConfigureAwait(false);
            return await _botContext.Segments.Where(s => s.User.UserId == userId).Select(s => s.SegmentId).ToArrayAsync().ConfigureAwait(false);
        }

        public async Task<bool> UserHasSegmentAsync(IUser user, int segmentId)
        {
            var userId = await _userRepository.GetUserIdAsync(user).ConfigureAwait(false);
            return await _botContext.Segments.AnyAsync(s => s.User.UserId == userId && s.SegmentId == segmentId).ConfigureAwait(false);
        }

        public async Task UpdateBalancesAsync()
        {
            var bank = await _botContext.GetBankAsync().ConfigureAwait(false);

            foreach (var dbSegment in await _botContext.Segments.Include(s => s.User).ToArrayAsync().ConfigureAwait(false))
            {
                var amount = bank.Balance / 100000000m + dbSegment.UnitsPerTick;
                dbSegment.User.Balance += amount;
                bank.Balance -= amount;
            }

            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<Dictionary<Resource, decimal>> GetSegmentResourcePerTickAsync(int segmentId)
        {
            return _botContext.GetSegmentResourcePerTickAsync(segmentId);
        }

        public Task<Dictionary<Resource, decimal>> GetSegmentResourcesAsync(int segmentId)
        {
            return _botContext.GetSegmentResourcesAsync(segmentId);
        }

        public async Task<IUser> GetOwnerAsync(int segmentId)
        {
            var segment = await _botContext.Segments.Include(s => s.User).FirstAsync(s => s.SegmentId == segmentId).ConfigureAwait(false);
            var discordUserId = segment.User.DiscordUserId;
            return await _client.GetUserAsync((ulong)discordUserId).ConfigureAwait(false);
        }
    }
}
