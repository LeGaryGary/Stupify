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
            var userId = await _userRepository.GetUserId(user);

            var dbUser = await _botContext.Users.FirstAsync(u => u.UserId == userId);
            var dbSegment = new SQL.Models.Segment
            {
                User = dbUser,
                EnergyPerTick = 0,
                UnitsPerTick = 0
            };
            _botContext.Segments.Add(dbSegment);
            await _botContext.SaveChangesAsync();

            await _segments.NewSegmentAsync(dbSegment.SegmentId);
            return (dbSegment.SegmentId, await _universeRepository.NewSegmentAsync(dbSegment.SegmentId));
        }

        public async Task<Segment> GetSegmentAsync(int segmentId)
        {
            return await _segments.GetAsync(segmentId);
        }

        public async Task SetSegmentAsync(int segmentId, Segment segment)
        {
            await _segments.SetAsync(segmentId, segment);
        }

        public async Task<(int x, int y)?> DeleteSegmentAsync(int segmentId)
        {
            var dbSegment = await _botContext.Segments.FirstAsync(s => s.SegmentId == segmentId);
            _botContext.Segments.Remove(dbSegment);
            await _botContext.SaveChangesAsync();

            await Task.Run(() => _segments.DeleteSegment(segmentId));
            return await _universeRepository.DeleteSegmentAsync(segmentId);
        }

        public async Task<Dictionary<BlockType, int>> ResetSegmentAsync(int segmentId)
        {
            return await _segments.ResetSegmentAsync(segmentId);
        }

        public async Task<bool> AddBlockAsync(int segmentId, int x, int y, BlockType blockType)
        {
            var segment = await _segments.GetAsync(segmentId);

            var addBlockResult = segment.AddBlock(x, y, blockType);

            if (!addBlockResult) return false;

            var dBSegment = await _botContext.Segments.FirstAsync(s => s.SegmentId == segmentId);
            dBSegment.EnergyPerTick = segment.ResourcePerTick(Resource.Energy);
            dBSegment.UnitsPerTick = segment.ResourcePerTick(Resource.Unit);
            await _botContext.SaveChangesAsync();

            await _segments.SetAsync(segmentId, segment);

            return true;
        }

        public async Task<BlockType?> DeleteBlockAsync(int segmentId, int x, int y)
        {
            var segment = await _segments.GetAsync(segmentId);
            var deleteBlockResult = segment.DeleteBlock(x, y);

            if (!deleteBlockResult.HasValue) return null;

            await _segments.SetAsync(segmentId, segment);

            var dBSegment = await _botContext.Segments.FirstAsync(s => s.SegmentId == segmentId);
            dBSegment.EnergyPerTick = segment.ResourcePerTick(Resource.Energy);
            dBSegment.UnitsPerTick = segment.ResourcePerTick(Resource.Unit);
            await _botContext.SaveChangesAsync();

            return deleteBlockResult;
        }

        public async Task<Dictionary<Resource, decimal>> GetOutput(int segmentId)
        {
            return (await _segments.GetAsync(segmentId)).ResourcePerTick();
        }

        public async Task<bool> Exists(int segmentId)
        {
            return await _botContext.Segments.AnyAsync(s => s.SegmentId == segmentId);
        }

        public async Task<int> SegmentCountAsync(IUser user)
        {
            var userId = await _userRepository.GetUserId(user);
            return await _botContext.Segments.CountAsync(s => s.User.UserId == userId);
        }

        public async Task<IEnumerable<Segment>> GetSegments(IUser user)
        {
            var userId = await _userRepository.GetUserId(user);

            var segments = new List<Segment>();
            foreach (var segment in await _botContext.Segments.Where(s => s.User.UserId == userId).ToArrayAsync())
            {
                segments.Add(await GetSegmentAsync(segment.SegmentId));
            }

            return segments;
        }

        public async Task<IEnumerable<int>> GetSegmentIds(IUser user)
        {
            var userId = await _userRepository.GetUserId(user);
            return await _botContext.Segments.Where(s => s.User.UserId == userId).Select(s => s.SegmentId).ToArrayAsync();
        }

        public async Task<bool> UserHasSegmentAsync(IUser user, int segmentId)
        {
            var userId = await _userRepository.GetUserId(user);
            return await _botContext.Segments.AnyAsync(s => s.User.UserId == userId && s.SegmentId == segmentId);
        }

        public async Task UpdateBalancesAsync()
        {
            var bank = await _botContext.GetBankAsync();

            foreach (var dbSegment in await _botContext.Segments.Include(s => s.User).ToArrayAsync())
            {
                var amount = bank.Balance / 100000000m + dbSegment.UnitsPerTick;
                dbSegment.User.Balance += amount;
                bank.Balance -= amount;
            }

            await _botContext.SaveChangesAsync();
        }

        public async Task<Dictionary<Resource, decimal>> GetSegmentResourcePerTickAsync(int segmentId)
        {
            return await _botContext.GetSegmentResourcePerTickAsync(segmentId);
        }

        public async Task<Dictionary<Resource, decimal>> GetSegmentResourcesAsync(int segmentId)
        {
            return await _botContext.GetSegmentResourcesAsync(segmentId);
        }

        public async Task<IUser> GetOwner(int segmentId)
        {
            var segment = await _botContext.Segments.Include(s => s.User).FirstAsync(s => s.SegmentId == segmentId);
            var discordUserId = segment.User.DiscordUserId;
            return await _client.GetUserAsync((ulong)discordUserId);
        }
    }
}
