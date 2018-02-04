using System.Diagnostics;
using System.Threading.Tasks;

using BotDataGraph.MessageAnalyser;

using Discord.Commands;
using Discord.WebSocket;

using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Client
{
    using BotDataGraph.MessageAnalyser.Models;

    internal static class MessageHandler
    {
        private static readonly Neo4JMessageHandler Neo4JMessageHandler;

        static MessageHandler()
        {
            if (Config.Neo4JMessageHandlerEnabled)
            {
                Neo4JMessageHandler = new Neo4JMessageHandler(Config.Neo4JUri, Config.Neo4JAuth);
            }
        }

        internal static async Task Handle(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message) || messageParam.Author.IsBot)
            {
                return;
            }

            var argPos = 0;
            var context = new SocketCommandContext(ClientManager.Client, message);

            var neo4JMessage = new Message
                {
                    Time = context.Message.CreatedAt.DateTime,
                    UserId = context.User.Id,
                    Content = context.Message.Content,
                    ServerId = context.Guild.Id,
                    ChannelId = context.Channel.Id
                };
            var addMessageNodeTask = Neo4JMessageHandler.Handle(neo4JMessage);

            using (var db = new BotContext())
            {
                var serverUser = await db.GetServerUserAsync(context.User.Id, context.Guild.Id, true);
                if (serverUser.Muted)
                {
                    var deleteTask = context.Message.DeleteAsync();
                    var dmChannel = await context.User.GetOrCreateDMChannelAsync();
                    await dmChannel.SendMessageAsync("You are muted, congrats muggle, you did it");
                    await deleteTask;
                    return;
                }
            }

            if (!(message.HasStringPrefix(Config.CommandPrefix, ref argPos)
                  || message.HasMentionPrefix(ClientManager.Client.CurrentUser, ref argPos)))
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            var result = await ClientManager.Commands.ExecuteAsync(context, argPos);
            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync("Command not found!");
                }
                else
                {
                    await ClientManager.LogAsync(result.ErrorReason);
                }
            }
            sw.Stop();
            await ClientManager.LogAsync("This command took " + sw.ElapsedMilliseconds + "ms", true);
            await addMessageNodeTask;
        }
    }
}