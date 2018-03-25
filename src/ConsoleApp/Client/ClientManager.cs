using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace StupifyConsoleApp.Client
{
    public class ClientManager
    {
        private readonly IDiscordClient _client;

        public ClientManager(IMessageHandler messageHandler, IReactionHandler segmentEditHandler, ILogger<ClientManager> logger, IDiscordClient client)
        {
            _client = client;

            switch (_client)
            {
                case DiscordSocketClient discordSocketClient:
                    discordSocketClient.MessageReceived += messageHandler.Handle;
                    discordSocketClient.ReactionAdded += segmentEditHandler.Handle;
                    discordSocketClient.Log += logMessage =>
                    {
                        switch (logMessage.Severity)
                        {
                            case LogSeverity.Critical:
                                logger.LogCritical(logMessage.Exception, logMessage.Message);
                                break;
                            case LogSeverity.Error:
                                logger.LogError(logMessage.Exception, logMessage.Message);
                                break;
                            case LogSeverity.Warning:
                                logger.LogWarning(logMessage.Exception, logMessage.Message);
                                break;
                            case LogSeverity.Info:
                                logger.LogInformation(logMessage.Exception, logMessage.Message);
                                break;
                            case LogSeverity.Verbose:
                                logger.LogDebug(logMessage.Exception, logMessage.Message);
                                break;
                            case LogSeverity.Debug:
                                logger.LogTrace(logMessage.Exception, logMessage.Message);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        return Task.CompletedTask;
                    };
                    break;
            }
        }

        public async Task Start()
        {
            switch (_client)
            {
                case DiscordSocketClient discordSocketClient:
                    await discordSocketClient.LoginAsync(TokenType.Bot, Config.DiscordBotUserToken);
                    break;
                case DiscordShardedClient discordShardedClient:
                    await discordShardedClient.LoginAsync(TokenType.Bot, Config.DiscordBotUserToken);
                    break;
            }
            
            await _client.StartAsync();
            while (true)
            {
                switch (_client)
                {
                    case DiscordShardedClient discordShardedClient:
                        await discordShardedClient.SetGameAsync($"{Config.CommandPrefix} help | Servers: {discordShardedClient.Guilds.Count}");
                        break;
                    case DiscordSocketClient discordSocketClient:
                        await discordSocketClient.SetGameAsync($"{Config.CommandPrefix} help | Servers: {discordSocketClient.Guilds.Count}");
                        break;
                }
                await Task.Delay(60000);
            }
        }
    }
}