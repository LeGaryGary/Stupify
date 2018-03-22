using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.DataModels;
using Direction = TicTacZap.Direction;

namespace StupifyConsoleApp.TicTacZapManagement
{
    public class GameState
    {
        private Dictionary<int, int?> UserSegmentSelection { get; } = new Dictionary<int, int?>();
        private Dictionary<int, int?> UserTemplateSelection { get; } = new Dictionary<int, int?>();
        internal List<(int attackingSegment, int defendingSegment, Direction direction, IUserMessage attackingmessage, Queue<string> battlefeed)> CurrentWars { get; } = new List<(int, int, Direction, IUserMessage, Queue<string>)>();
        public int Tick { get; set; }

        public async Task<bool> SetUserSegmentSelection(int userId, int segmentId, BotContext db)
        {
            if (!UserSegmentSelection.ContainsKey(userId)) UserSegmentSelection.Add(userId, null);
            if (!await db.Segments.AnyAsync(s => s.User.UserId == userId && s.SegmentId == segmentId)) return false;
            UserSegmentSelection[userId] = segmentId;
            return true;
        }

        public int? GetUserSegmentSelection(int userId)
        {
            if (UserSegmentSelection.ContainsKey(userId)) return UserSegmentSelection[userId];
            UserSegmentSelection.Add(userId, null);
            return null;
        }

        public async Task<bool> SetUserTemplateSelection(int userId, int templateId, BotContext db)
        {
            if (!UserTemplateSelection.ContainsKey(userId)) UserTemplateSelection.Add(userId, null);
            if (!await db.SegmentTemplates.AnyAsync(s => s.User.UserId == userId && s.SegmentTemplateId == templateId)) return false;
            UserTemplateSelection[userId] = templateId;
            return true;
        }

        public int? GetUserTemplateSelection(int userId)
        {
            if (UserTemplateSelection.ContainsKey(userId)) return UserTemplateSelection[userId];
            UserTemplateSelection.Add(userId, null);
            return null;
        }
    }
}