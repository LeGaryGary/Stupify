using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.Commands;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;
using TicTacZap;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.Client
{
    public class SegmentEditReactionHandler : IReactionHandler
    {
        private enum State {Move, Select}

        private static readonly Dictionary<ulong, OwnerInfo> Owners = new Dictionary<ulong, OwnerInfo>();
        private readonly IDiscordClient _client;
        private readonly BotContext _db;
        private readonly TicTacZapController _ticTacZapController;

        public SegmentEditReactionHandler(IDiscordClient client, BotContext db, TicTacZapController ticTacZapController)
        {
            _client = client;
            _db = db;
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
                                await RemoveBlock(owner);
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

                        await UpdateMsg(message.Value);
                        break;
                    case State.Select:
                        var comment = "";
                        switch (reaction.Emote.Name)
                        {

                            case "❌":
                                await RemoveBlock(owner);
                                break;
                            case "↩":
                                await SetState(message.Value, State.Move);
                                break;
                            case "🛡":
                                comment = await AddBlock(owner, BlockType.Wall);
                                break;
                            case "⚡":
                                comment = await AddBlock(owner, BlockType.Energy);
                                break;
                            case "🗼":
                                comment = await AddBlock(owner, BlockType.Beamer);
                                break;
                        }

                        await UpdateMsg(message.Value, comment);
                        break;
                }

                await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            }
        }

        private async Task UpdateMsg(IUserMessage message, string comment = "")
        {
            var owner = Owners[message.Id];

            var selection =
                await _ticTacZapController.RenderBlockInfoAsync(owner.SegmentId, owner.Position.Item2, 8-owner.Position.Item1) ??
                "Empty block";
            var header = $"--- Selection ---\n{selection}";

            var body = await _ticTacZapController.RenderSegmentAsync(owner.SegmentId,
                          new Tuple<int, int>(owner.Position.Item1, owner.Position.Item2));

            await message.ModifyAsync(m => m.Content = $"{comment}\n```{header}\n\n{body}```");
        }

        private async Task SetState(IUserMessage message, State state)
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
                    await message.AddReactionAsync(new Emoji("🛡"));
                    await message.AddReactionAsync(new Emoji("⚡"));
                    await message.AddReactionAsync(new Emoji("🗼"));
                    await message.AddReactionAsync(new Emoji("❌"));
                    await message.AddReactionAsync(new Emoji("↩"));
                    break;
            }
        }

        private async Task<string> AddBlock(OwnerInfo owner, BlockType type)
        {
            if (!await Inventories.RemoveFromInventoryAsync(type, 1, owner.DbUserId)) return Responses.ShopAdvisoryMessage;
            await RemoveBlock(owner);
            await Segments.AddBlockAsync(owner.SegmentId, owner.Position.Item2, 8-owner.Position.Item1, type);
            await SaveChangesToDb(owner);
            return "";
        }

        private async Task RemoveBlock(OwnerInfo owner)
        {
            var blockType = await Segments.DeleteBlockAsync(owner.SegmentId, owner.Position.Item2, 8-owner.Position.Item1);
            if (blockType != null)
            {
                await Inventories.AddToInventoryAsync(blockType.Value, 1, owner.DbUserId);
                await SaveChangesToDb(owner);
            }
        }

        private async Task SaveChangesToDb(OwnerInfo owner)
        {
            var dbSegment = await _db.Segments.FirstAsync(s => s.SegmentId == owner.SegmentId);
            var outputs = await Segments.GetOutput(owner.SegmentId);

            dbSegment.EnergyPerTick = outputs[Resource.Energy];
            dbSegment.UnitsPerTick = outputs[Resource.Unit];

            await _db.SaveChangesAsync();
        }

        public async Task NewOwner(IUserMessage message, int segmentId, ulong userId, int dbUserId)
        {
            Owners.Add(message.Id, new OwnerInfo(userId, dbUserId, segmentId));
            await UpdateMsg(message);
            await SetState(message, State.Move);
        }

        private class OwnerInfo
        {
            public State State { get; set; }
            public ulong UserId { get; }
            public int DbUserId { get; }
            public int SegmentId { get; }
            public Tuple<int, int> Position { get; set; }

            public OwnerInfo(ulong userId, int dbUserId, int segmentId)
            {
                UserId = userId;
                SegmentId = segmentId;
                DbUserId = dbUserId;
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
