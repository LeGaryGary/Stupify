using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp.Commands.Hotkeys
{
    internal class MoveSelectorUpHotKey : IHotkey
    {
        private readonly SegmentEditReactionHandler _segmentEditReactionHandler;

        public MoveSelectorUpHotKey(SegmentEditReactionHandler segmentEditReactionHandler)
        {
            _segmentEditReactionHandler = segmentEditReactionHandler;
        }

        public char Key => 'w';

        public async Task<bool> ExecuteAsync(ICommandContext context)
        {
            var messageOwnerInfo =  SegmentEditReactionHandler.Owners.FirstOrDefault(o => o.Value.UserId == context.User.Id);

            if (messageOwnerInfo.Value == null) return false;

            var position = messageOwnerInfo.Value.Position;
            position.y--;
            messageOwnerInfo.Value.Position = position;

            var userMessage = (IUserMessage)await context.Channel.GetMessageAsync(messageOwnerInfo.Key).ConfigureAwait(false);
            await _segmentEditReactionHandler.UpdateMsg(userMessage).ConfigureAwait(false);
            return true;
        }
    }

    internal class MoveSelectorDownHotKey : IHotkey
    {
        private readonly SegmentEditReactionHandler _segmentEditReactionHandler;

        public MoveSelectorDownHotKey(SegmentEditReactionHandler segmentEditReactionHandler)
        {
            _segmentEditReactionHandler = segmentEditReactionHandler;
        }

        public char Key => 's';

        public async Task<bool> ExecuteAsync(ICommandContext context)
        {
            var messageOwnerInfo =  SegmentEditReactionHandler.Owners.FirstOrDefault(o => o.Value.UserId == context.User.Id);

            if (messageOwnerInfo.Value == null) return false;

            var position = messageOwnerInfo.Value.Position;
            position.y++;
            messageOwnerInfo.Value.Position = position;

            var userMessage = (IUserMessage)await context.Channel.GetMessageAsync(messageOwnerInfo.Key).ConfigureAwait(false);
            await _segmentEditReactionHandler.UpdateMsg(userMessage).ConfigureAwait(false);
            return true;
        }
    }

    internal class MoveSelectorLeftHotKey : IHotkey
    {
        private readonly SegmentEditReactionHandler _segmentEditReactionHandler;

        public MoveSelectorLeftHotKey(SegmentEditReactionHandler segmentEditReactionHandler)
        {
            _segmentEditReactionHandler = segmentEditReactionHandler;
        }

        public char Key => 'a';

        public async Task<bool> ExecuteAsync(ICommandContext context)
        {
            var messageOwnerInfo =  SegmentEditReactionHandler.Owners.FirstOrDefault(o => o.Value.UserId == context.User.Id);

            if (messageOwnerInfo.Value == null) return false;

            var position = messageOwnerInfo.Value.Position;
            position.x--;
            messageOwnerInfo.Value.Position = position;

            var userMessage = (IUserMessage)await context.Channel.GetMessageAsync(messageOwnerInfo.Key).ConfigureAwait(false);
            await _segmentEditReactionHandler.UpdateMsg(userMessage).ConfigureAwait(false);
            return true;
        }
    }

    internal class MoveSelectorRightHotKey : IHotkey
    {
        private readonly SegmentEditReactionHandler _segmentEditReactionHandler;

        public MoveSelectorRightHotKey(SegmentEditReactionHandler segmentEditReactionHandler)
        {
            _segmentEditReactionHandler = segmentEditReactionHandler;
        }

        public char Key => 'd';

        public async Task<bool> ExecuteAsync(ICommandContext context)
        {
            var messageOwnerInfo =  SegmentEditReactionHandler.Owners.FirstOrDefault(o => o.Value.UserId == context.User.Id);

            if (messageOwnerInfo.Value == null) return false;

            var position = messageOwnerInfo.Value.Position;
            position.x++;
            messageOwnerInfo.Value.Position = position;

            var userMessage = (IUserMessage)await context.Channel.GetMessageAsync(messageOwnerInfo.Key).ConfigureAwait(false);
            await _segmentEditReactionHandler.UpdateMsg(userMessage).ConfigureAwait(false);
            return true;
        }
    }
}
