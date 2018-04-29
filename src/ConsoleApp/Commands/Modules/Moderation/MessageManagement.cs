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
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public Task PurgeChannelCommand()
        {
            if (!(Context.Channel is ITextChannel channel)) return Task.CompletedTask;
            var messagesPaged = Context.Channel.GetMessagesAsync(DefaultDelete).Select(m => m.Select(mInner => mInner.Id));
            return messagesPaged.ForEachAsync(messageIds => channel.DeleteMessagesAsync(messageIds));
        }

        [Command("PurgeChannel")]
        [Moderator]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public Task PurgeChannelCommand(int messagesToDelete)
        {
            if (!(Context.Channel is ITextChannel channel)) return Task.CompletedTask;
            var messagesPaged = Context.Channel.GetMessagesAsync(messagesToDelete).Select(m => m.Select(mInner => mInner.Id));
            return messagesPaged.ForEachAsync(messageIds => channel.DeleteMessagesAsync(messageIds));
        }

        [Command("PurgeChannel")]
        [Moderator]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public Task PurgeChannelCommand(IGuildUser userMessageToPurge)
        {
            if (!(Context.Channel is ITextChannel channel)) return Task.CompletedTask;
            var messagesPaged = Context.Channel.GetMessagesAsync(DefaultDelete).Select(m => m.Where(mInner => mInner.Author == userMessageToPurge).Select(mInner => mInner.Id));
            return messagesPaged.ForEachAsync(messageIds => channel.DeleteMessagesAsync(messageIds));
        }

        [Command("PurgeChannel")]
        [Moderator]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public Task PurgeChannelCommand(IGuildUser userMessageToPurge, int messagesToDelete)
        {
            if (!(Context.Channel is ITextChannel channel)) return Task.CompletedTask;
            var messagesPaged = Context.Channel.GetMessagesAsync(messagesToDelete).Select(m => m.Where(mInner => mInner.Author == userMessageToPurge).Select(mInner => mInner.Id));
            return messagesPaged.ForEachAsync(messageIds => channel.DeleteMessagesAsync(messageIds));
        }

        [Command("PurgeServer")]
        [Moderator]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeServerCommand(IGuildUser userMessageToPurge)
        {
            var channels = await Context.Guild.GetTextChannelsAsync().ConfigureAwait(false);
            foreach (var channel in channels)
            {
                var messagesPaged = channel.GetMessagesAsync(DefaultDelete).Select(m => m.Where(mInner => mInner.Author == userMessageToPurge).Select(mInner => mInner.Id));
                await messagesPaged.ForEachAsync(messageIds => channel.DeleteMessagesAsync(messageIds)).ConfigureAwait(false);
            }
        }

        [Command("PurgeServer")]
        [Moderator]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeServerCommand(IGuildUser userMessageToPurge, int messagesToDelete)
        {
            var channels = await Context.Guild.GetTextChannelsAsync().ConfigureAwait(false);
            foreach (var channel in channels)
            {
                var messagesPaged = channel.GetMessagesAsync(messagesToDelete).Select(m => m.Where(mInner => mInner.Author == userMessageToPurge).Select(mInner => mInner.Id));
                await messagesPaged.ForEachAsync(messageIds => channel.DeleteMessagesAsync(messageIds)).ConfigureAwait(false);
            }
        }
    }
}
