using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Client
{
    public class SegmentEditReactionHandler : IReactionHandler
    {
        private static readonly Dictionary<ulong, OwnerInfo> Owners = new Dictionary<ulong, OwnerInfo>();
        private readonly IDiscordClient _client;
        private readonly TicTacZapController _ticTacZapController;

        public SegmentEditReactionHandler(IDiscordClient client, TicTacZapController ticTacZapController)
        {
            _client = client;
            _ticTacZapController = ticTacZapController;
        }

        public async Task Handle(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId != _client.CurrentUser.Id && Owners.ContainsKey(message.Value.Id))
            {
                var owner = Owners[message.Value.Id];
                if (reaction.UserId != owner.UserId)
                {
                    await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    return;
                }

                var y = owner.Position.Item1;
                var x = owner.Position.Item2;
                switch (reaction.Emote.Name)
                {
                    case "❌":
                        await message.Value.RemoveAllReactionsAsync();
                        return;
                    case "⬆":
                        if(y > 0) Owners[message.Value.Id].Position = new Tuple<int, int>(--y, x);
                        break;
                    case "⬇":
                        if (y < 8) Owners[message.Value.Id].Position = new Tuple<int, int>(++y, x);
                        break;
                    case "➡":
                        if (x < 8) Owners[message.Value.Id].Position = new Tuple<int, int>(y, ++x);
                        break;
                    case "⬅":
                        if (x > 0) Owners[message.Value.Id].Position = new Tuple<int, int>(y, --x);
                        break;
                }
                await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

                var str = await _ticTacZapController.RenderSegmentAsync(owner.SegmentId, new Tuple<int, int>(y, x));
                await message.Value.ModifyAsync(m => m.Content = $"```{str}```");
            }
        }

        public static void NewOwner(ulong messageId, int segmentId, ulong userId)
        {
            Owners.Add(messageId, new OwnerInfo(userId, segmentId));
        }

        private class OwnerInfo
        {
            public ulong UserId { get; }
            public int SegmentId { get; }
            public Tuple<int, int> Position { get; set; }

            public OwnerInfo(ulong userId, int segmentId)
            {
                UserId = userId;
                SegmentId = segmentId;
                Position = new Tuple<int, int>(0, 0);
            }
        }
    }

    public interface IReactionHandler
    {
        Task Handle(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction);
    }
}
