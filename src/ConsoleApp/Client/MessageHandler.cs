using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Client
{
    public class MessageHandler: IMessageHandler
    {
        private readonly BotContext _db;
        private readonly IDiscordClient _client;
        private readonly CommandService _commandService;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(BotContext db, IDiscordClient client, CommandService commandService, ILogger<MessageHandler> logger)
        {
            _db = db;
            _client = client;
            _commandService = commandService;
            _logger = logger;
        }

        public async Task Handle(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message) || messageParam.Author.IsBot) return;

            var argPos = 0;
            var context = new CommandContext(_client, message);
            
            var serverUser = await _db.GetServerUserAsync(context.User.Id, context.Guild.Id, true);
            if (serverUser.Muted)
            {
                await context.Message.DeleteAsync();
                return;
            }

            if (!(message.HasStringPrefix(Config.CommandPrefix + " ", ref argPos)
                  || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            var result = await _commandService.ExecuteAsync(context, argPos, Config.ServiceProvider);
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case CommandError.UnknownCommand:
                        if (Config.Debug)await context.Channel.SendMessageAsync("Command not found!");
                        else return;
                        break;
                    case CommandError.BadArgCount:
                    case CommandError.ParseFailed:
                        await context.Channel.SendMessageAsync("That's not right!");
                        break;
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        break;
                    default:
                        _logger.LogError("The message: {Message} \r\nHas caused the following error: {ErrorReason}\r\nIn the server: {Guild}", context.Message, result.ErrorReason, context.Guild.Name);
                        await context.Channel.SendMessageAsync(
                            "Internal error! You may shout at the developers here: https://discord.gg/nb5rUhd");
                        break;
                }
            sw.Stop();

            _logger.LogTrace("Command {Message} in {Guild} took {ElapsedMilliseconds}ms", context.Message, context.Guild.Name, sw.ElapsedMilliseconds);
            
            await context.Message.DeleteAsync();
        }
    }

    public interface IMessageHandler
    {
        Task Handle(SocketMessage messageParam);
    }
}