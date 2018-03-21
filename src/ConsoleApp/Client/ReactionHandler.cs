using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using StupifyConsoleApp.Commands;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Client
{
    public class SegmentEditReactionHandler : IReactionHandler
    {
        private enum State { Move, Select}

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
            if (reaction.UserId != _client.CurrentUser.Id && Owners.ContainsKey(message.Id))
            {
                var owner = Owners[message.Id];
                if (reaction.UserId != owner.UserId)
                {
                    await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    return;
                }

                var y = owner.Position.Item1;
                var x = owner.Position.Item2;
                switch (owner.State)
                {
                    case State.Move:
                        switch (reaction.Emote.Name)
                        {
                            case "✅":
                                Owners.Remove(message.Id);
                                await message.Value.RemoveAllReactionsAsync();
                                return;
                            case "🏗":
                                await SetState(message.Value, State.Select);
                                break;
                            case "❌":
                                
                                break;
                            case "⬆":
                                if (y > 0) Owners[message.Id].Position = new Tuple<int, int>(--y, x);
                                break;
                            case "⬇":
                                if (y < 8) Owners[message.Id].Position = new Tuple<int, int>(++y, x);
                                break;
                            case "➡":
                                if (x < 8) Owners[message.Id].Position = new Tuple<int, int>(y, ++x);
                                break;
                            case "⬅":
                                if (x > 0) Owners[message.Id].Position = new Tuple<int, int>(y, --x);
                                break;
                        }

                        var str = await _ticTacZapController.RenderSegmentAsync(owner.SegmentId, new Tuple<int, int>(y, x));
                        await message.Value.ModifyAsync(m => m.Content = $"```{str}```");
                        break;
                    case State.Select:
                        switch (reaction.Emote.Name)
                        {

                            case "❌":
                                
                                break;
                            case "↩":
                                await SetState(message.Value, State.Move);
                                break;
                        }

                        break;
                }

                await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            }
        }

        private static async Task SetState(IUserMessage message, State state)
        {
            Owners[message.Id].State = state;
            await message.RemoveAllReactionsAsync();
            switch (state)
            {
                case State.Move:
                    await message.AddReactionAsync(new Emoji("⬆"));
                    await message.AddReactionAsync(new Emoji("⬇"));
                    await message.AddReactionAsync(new Emoji("⬅"));
                    await message.AddReactionAsync(new Emoji("➡"));
                    await message.AddReactionAsync(new Emoji("❌"));
                    await message.AddReactionAsync(new Emoji("🏗"));
                    await message.AddReactionAsync(new Emoji("✅"));
                    break;
                case State.Select:
                    await message.AddReactionAsync(new Emoji("❌"));
                    await message.AddReactionAsync(new Emoji("↩"));
                    break;
            }
        }

        public static async Task NewOwner(IUserMessage message, int segmentId, ulong userId)
        {
            Owners.Add(message.Id, new OwnerInfo(userId, segmentId));
            await SetState(message, State.Move);
        }

        private class OwnerInfo
        {
            public State State { get; set; }
            public ulong UserId { get; }
            public int SegmentId { get; }
            public Tuple<int, int> Position { get; set; }

            public OwnerInfo(ulong userId, int segmentId)
            {
                UserId = userId;
                SegmentId = segmentId;
                State = State.Move;
                Position = new Tuple<int, int>(0, 0);
            }
        }
    }

    public interface IReactionHandler
    {
        Task Handle(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction);
    }
}
