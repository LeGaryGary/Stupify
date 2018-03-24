using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Direction = TicTacZap.Direction;

namespace StupifyConsoleApp.TicTacZapManagement
{
    public class GameState
    {
        private Dictionary<int, int?> UserSegmentSelection { get; } = new Dictionary<int, int?>();
        private Dictionary<int, int?> UserTemplateSelection { get; } = new Dictionary<int, int?>();
        internal List<(int attackingSegment, int defendingSegment, Direction direction, IUserMessage attackingmessage, Queue<string> battlefeed)> CurrentWars { get; } = new List<(int, int, Direction, IUserMessage, Queue<string>)>();
        public int Tick { get; set; }

        public void SetUserSegmentSelection(int userId, int segmentId)
        {
            if (!UserSegmentSelection.ContainsKey(userId)) UserSegmentSelection.Add(userId, null);
            UserSegmentSelection[userId] = segmentId;
        }

        public int? GetUserSegmentSelection(int userId)
        {
            if (UserSegmentSelection.ContainsKey(userId)) return UserSegmentSelection[userId];
            UserSegmentSelection.Add(userId, null);
            return null;
        }

        public void SetUserTemplateSelection(int userId, int templateId)
        {
            if (!UserTemplateSelection.ContainsKey(userId)) UserTemplateSelection.Add(userId, null);
            UserTemplateSelection[userId] = templateId;
        }

        public int? GetUserTemplateSelection(int userId)
        {
            if (UserTemplateSelection.ContainsKey(userId)) return UserTemplateSelection[userId];
            UserTemplateSelection.Add(userId, null);
            return null;
        }
    }
}