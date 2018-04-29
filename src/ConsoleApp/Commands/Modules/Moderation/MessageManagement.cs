using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using StupifyConsoleApp.Commands.Conditions;

namespace StupifyConsoleApp.Commands.Modules.Moderation
{
    public class MessageManagement : ModuleBase<CommandContext>
    {
        private const int DefaultDelete = 10000;

        [Command("PurgeChannel")]
        [Moderator]
        public Task PurgeChannelCommand()
        {
            var messagesPaged = Context.Channel.GetMessagesAsync(DefaultDelete).Select(m => m.Select(mInner => mInner.Id));
            return messagesPaged.ForEachAsync(messageIds => Context.Channel.DeleteMessagesAsync(messageIds));
        }

        [Command("PurgeChannel")]
        [Moderator]
        public Task PurgeChannelCommand(int messagesToDelete)
        {
            var messagesPaged = Context.Channel.GetMessagesAsync(messagesToDelete).Select(m => m.Select(mInner => mInner.Id));
            return messagesPaged.ForEachAsync(messageIds => Context.Channel.DeleteMessagesAsync(messageIds));
        }

        [Command("PurgeChannel")]
        [Moderator]
        public Task PurgeChannelCommand(IGuildUser userMessageToPurge)
        {
            var messagesPaged = Context.Channel.GetMessagesAsync(DefaultDelete).Select(m => m.Where(mInner => mInner.Author == userMessageToPurge).Select(mInner => mInner.Id));
            return messagesPaged.ForEachAsync(messageIds => Context.Channel.DeleteMessagesAsync(messageIds));
        }

        [Command("PurgeChannel")]
        [Moderator]
        public Task PurgeChannelCommand(IGuildUser userMessageToPurge, int messagesToDelete)
        {
            var messagesPaged = Context.Channel.GetMessagesAsync(messagesToDelete).Select(m => m.Where(mInner => mInner.Author == userMessageToPurge).Select(mInner => mInner.Id));
            return messagesPaged.ForEachAsync(messageIds => Context.Channel.DeleteMessagesAsync(messageIds));
        }

        [Command("PurgeServer")]
        [Moderator]
        public async Task PurgeServerCommand(IGuildUser userMessageToPurge)
        {
            var channels = await Context.Guild.GetTextChannelsAsync().ConfigureAwait(false);
            foreach (var channel in channels)
            {
                var messagesPaged = channel.GetMessagesAsync(DefaultDelete).Select(m => m.Where(mInner => mInner.Author == userMessageToPurge).Select(mInner => mInner.Id));
                await messagesPaged.ForEachAsync(messageIds => Context.Channel.DeleteMessagesAsync(messageIds)).ConfigureAwait(false);
            }
        }

        [Command("PurgeServer")]
        [Moderator]
        public async Task PurgeServerCommand(IGuildUser userMessageToPurge, int messagesToDelete)
        {
            var channels = await Context.Guild.GetTextChannelsAsync().ConfigureAwait(false);
            foreach (var channel in channels)
            {
                var messagesPaged = channel.GetMessagesAsync(messagesToDelete).Select(m => m.Where(mInner => mInner.Author == userMessageToPurge).Select(mInner => mInner.Id));
                await messagesPaged.ForEachAsync(messageIds => Context.Channel.DeleteMessagesAsync(messageIds)).ConfigureAwait(false);
            }
        }
    }
}
