using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Client
{
    internal static class ReactionHandler
    {
        private static readonly Dictionary<ulong, EditOwnerInfo> EditOwners = new Dictionary<ulong, EditOwnerInfo>();

        internal static async Task HandleSegmentEdit(Cacheable<IUserMessage, ulong> message,
            ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId != ClientManager.Client.CurrentUser.Id && EditOwners.ContainsKey(message.Value.Id))
            {
                if (reaction.User.Value.Id != EditOwners[message.Id].UserID)
                {
                    await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    return;
                }

                if (reaction.Emote.Name == "❌")
                {
                    await message.Value.RemoveAllReactionsAsync();
                    EditOwners.Remove(message.Value.Id);
                    return;
                }

                if (reaction.Emote.Name == "⬆")
                {
                    var tmp1 = EditOwners[message.Id].Position.Item1;
                    var tmp2 = EditOwners[message.Id].Position.Item2;
                    if (tmp1 > 0) EditOwners[message.Id].Position = new Tuple<int, int>(--tmp1, tmp2);
                }
                else if (reaction.Emote.Name == "⬇")
                {
                    var tmp1 = EditOwners[message.Id].Position.Item1;
                    var tmp2 = EditOwners[message.Id].Position.Item2;
                    if (tmp1 < 8) EditOwners[message.Id].Position = new Tuple<int, int>(++tmp1, tmp2);
                }
                else if (reaction.Emote.Name == "➡")
                {
                    var tmp1 = EditOwners[message.Id].Position.Item1;
                    var tmp2 = EditOwners[message.Id].Position.Item2;
                    if (tmp2 < 8) EditOwners[message.Id].Position = new Tuple<int, int>(tmp1, ++tmp2);
                }
                else if (reaction.Emote.Name == "⬅")
                {
                    var tmp1 = EditOwners[message.Id].Position.Item1;
                    var tmp2 = EditOwners[message.Id].Position.Item2;
                    if (tmp2 > 0) EditOwners[message.Id].Position = new Tuple<int, int>(tmp1, --tmp2);
                }

                await ClientManager.LogAsync($"{EditOwners[message.Id].Position.Item1} {EditOwners[message.Id].Position.Item2}");
                await message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            }
        }

        internal static void NewEditOwner(ulong messageID, ulong userID)
        {
            EditOwners.Add(messageID, new EditOwnerInfo(userID, new Tuple<int, int>(0, 0)));
        }

        class EditOwnerInfo
        {
            public ulong UserID { get; }
            public Tuple<int, int> Position { get; set; }

            public EditOwnerInfo(ulong userID, Tuple<int, int> pos)
            {
                UserID = userID;
                Position = pos;
            }
        }
    }
}
