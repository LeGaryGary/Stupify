using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Stupify.Data.Repositories;
using StupifyConsoleApp.Commands;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.Client
{
    public class SegmentEditReactionHandler : IReactionHandler
    {
        internal enum State {Select}
        internal static Dictionary<ulong, OwnerInfo> Owners { get; } = new Dictionary<ulong, OwnerInfo>();
        private readonly IDiscordClient _client;
        private readonly TicTacZapController _ticTacZapController;
        private readonly ISegmentRepository _segmentRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public SegmentEditReactionHandler(IDiscordClient client, TicTacZapController ticTacZapController, ISegmentRepository segmentRepository, IInventoryRepository inventoryRepository)
        {
            _client = client;
            _ticTacZapController = ticTacZapController;
            _segmentRepository = segmentRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task HandleAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId != _client.CurrentUser.Id && Owners.ContainsKey(message.Id))
            {
                var owner = Owners[message.Id];
                if (reaction.UserId != owner.UserId)
                {
                    await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value).ConfigureAwait(false);
                    return;
                }

                switch (owner.State)
                {
                    case State.Select:
                        var comment = "";
                        switch (reaction.Emote.Name)
                        {

                            case "❌":
                                await RemoveBlockAsync(owner).ConfigureAwait(false);
                                break;
                            case "🛡":
                                comment = await AddBlockAsync(owner, BlockType.Wall).ConfigureAwait(false);
                                break;
                            case "⚡":
                                comment = await AddBlockAsync(owner, BlockType.Energy).ConfigureAwait(false);
                                break;
                            case "🗼":
                                comment = await AddBlockAsync(owner, BlockType.Beamer).ConfigureAwait(false);
                                break;
                            case "✅":
                                Owners.Remove(message.Id);
                                await message.Value.RemoveAllReactionsAsync().ConfigureAwait(false);
                                return;
                        }

                        await UpdateMsgAsync(message.Value, comment).ConfigureAwait(false);
                        break;
                }

                await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value).ConfigureAwait(false);
            }
        }

        public async Task UpdateMsgAsync(IUserMessage message, string comment = "")
        {
            var owner = Owners[message.Id];

            var selection =
                await _ticTacZapController.RenderBlockInfoAsync(owner.SegmentId, owner.Position.x, 8-owner.Position.y).ConfigureAwait(false) ??
                "Empty block";
            var header = $"--- Selection ---\n{selection}";

            var body = await _ticTacZapController.RenderSegmentAsync(owner.SegmentId,
                (owner.Position.x, owner.Position.y)).ConfigureAwait(false);

            await message.ModifyAsync(m => m.Content = $"{comment}\n```{header}\n\n{body}```").ConfigureAwait(false);
        }

        private async Task SetStateAsync(IUserMessage message, State state)
        {
            Owners[message.Id].State = state;
            await message.RemoveAllReactionsAsync().ConfigureAwait(false);
            switch (state)
            {
                case State.Select:
                    await message.AddReactionAsync(new Emoji("🛡")).ConfigureAwait(false);
                    await message.AddReactionAsync(new Emoji("⚡")).ConfigureAwait(false);
                    await message.AddReactionAsync(new Emoji("🗼")).ConfigureAwait(false);
                    await message.AddReactionAsync(new Emoji("❌")).ConfigureAwait(false);
                    await message.AddReactionAsync(new Emoji("✅")).ConfigureAwait(false);
                    break;
            }
        }

        private async Task<string> AddBlockAsync(OwnerInfo owner, BlockType type)
        {
            var user = await _client.GetUserAsync(owner.UserId).ConfigureAwait(false);
            if (!await _inventoryRepository.RemoveFromInventoryAsync(type, 1, user).ConfigureAwait(false)) return Responses.ShopAdvisoryMessage;
            await RemoveBlockAsync(owner).ConfigureAwait(false);
            await _segmentRepository.AddBlockAsync(owner.SegmentId, owner.Position.x, 8-owner.Position.y, type).ConfigureAwait(false);
            return "";
        }

        private async Task RemoveBlockAsync(OwnerInfo owner)
        {
            var blockType = await _segmentRepository.DeleteBlockAsync(owner.SegmentId, owner.Position.x, 8-owner.Position.y).ConfigureAwait(false);
            if (blockType != null)
            {
                var user = await _client.GetUserAsync(owner.UserId).ConfigureAwait(false);
                await _inventoryRepository.AddToInventoryAsync(blockType.Value, 1, user).ConfigureAwait(false);
            }
        }

        public async Task NewOwnerAsync(IUserMessage message, int segmentId, ulong userId, int dbUserId)
        {
            Owners.Add(message.Id, new OwnerInfo(userId, dbUserId, segmentId));
            await UpdateMsgAsync(message).ConfigureAwait(false);
            await SetStateAsync(message, State.Select).ConfigureAwait(false);
        }

        internal class OwnerInfo
        {
            public State State { get; set; }
            public ulong UserId { get; }
            public int DbUserId { get; }
            public int SegmentId { get; }
            public (int x, int y) Position { get; set; }

            public OwnerInfo(ulong userId, int dbUserId, int segmentId)
            {
                UserId = userId;
                SegmentId = segmentId;
                DbUserId = dbUserId;
                State = State.Select;
                Position = (0, 0);
            }
        }
    }

    public interface IReactionHandler
    {
        Task HandleAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction);
        Task NewOwnerAsync(IUserMessage msg, int segmentId, ulong userId, int dbUserId);
    }
}
