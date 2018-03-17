using System.Diagnostics;
using System.Threading.Tasks;
using BotDataGraph.MessageAnalyser;
using BotDataGraph.MessageAnalyser.Models;
using Discord.Commands;
using Discord.WebSocket;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Client
{
    internal static class MessageHandler
    {
        private static readonly Neo4JMessageHandler Neo4JMessageHandler;

        static MessageHandler()
        {
            if (Config.Neo4JMessageHandlerEnabled)
                Neo4JMessageHandler = new Neo4JMessageHandler(Config.Neo4JUri, Config.Neo4JAuth);
        }

        internal static async Task Handle(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message) || messageParam.Author.IsBot) return;

            var argPos = 0;
            var context = new SocketCommandContext(ClientManager.Client, message);

            var addMessageNodeTask = PassMessageToNeo4J(context);

            using (var db = new BotContext())
            {
                var serverUser = await db.GetServerUserAsync(context.User.Id, context.Guild.Id, true);
                if (serverUser.Muted)
                {
                    await context.Message.DeleteAsync();
                    await addMessageNodeTask;
                    return;
                }
            }

            if (!(message.HasStringPrefix(Config.CommandPrefix + " ", ref argPos)
                  || message.HasMentionPrefix(ClientManager.Client.CurrentUser, ref argPos)))
            {
                await addMessageNodeTask;
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            var result = await ClientManager.Commands.ExecuteAsync(context, argPos);
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case CommandError.UnknownCommand when Config.Debug:
                        await context.Channel.SendMessageAsync("Command not found!");
                        break;
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync("That's not right!");
                        break;
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        break;
                    case CommandError.ParseFailed:
                        await context.Channel.SendMessageAsync("That's not right!");
                        break;
                    default:
                        await ClientManager.LogAsync(
                            $"\r\nThe message: {context.Message} \r\nHas caused the following error: {result.ErrorReason}\r\nIn the server: {context.Guild.Name}");
                        await context.Channel.SendMessageAsync(
                            "Internal error! You may shout at the developers here: https://discord.gg/nb5rUhd");
                        break;
                }
            sw.Stop();
            await ClientManager.LogAsync(
                $"Command \"{context.Message}\" in \"{context.Guild.Name}\" took " + sw.ElapsedMilliseconds + "ms",
                true);
            await addMessageNodeTask;
        }

        private static async Task PassMessageToNeo4J(SocketCommandContext context)
        {
            if (Config.Neo4JMessageHandlerEnabled)
            {
                var neo4JMessage = new Message
                {
                    Time = context.Message.CreatedAt.DateTime,
                    UserId = context.User.Id,
                    Content = context.Message.Content,
                    ServerId = context.Guild.Id,
                    ChannelId = context.Channel.Id
                };
                await Neo4JMessageHandler.Handle(neo4JMessage);
            }
        }
    }
}