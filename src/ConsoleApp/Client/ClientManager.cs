using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace StupifyConsoleApp.Client
{
    public class ClientManager
    {
        private readonly ILogger<ClientManager> _logger;
        private readonly IDiscordClient _client;

        public ClientManager(IMessageHandler messageHandler, SegmentEditReactionHandler segmentEditHandler, ILogger<ClientManager> logger, IDiscordClient client)
        {
            _logger = logger;
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
                                _logger.LogCritical(logMessage.Exception, logMessage.Message);
                                break;
                            case LogSeverity.Error:
                                _logger.LogError(logMessage.Exception, logMessage.Message);
                                break;
                            case LogSeverity.Warning:
                                _logger.LogWarning(logMessage.Exception, logMessage.Message);
                                break;
                            case LogSeverity.Info:
                                _logger.LogInformation(logMessage.Exception, logMessage.Message);
                                break;
                            case LogSeverity.Verbose:
                                _logger.LogDebug(logMessage.Exception, logMessage.Message);
                                break;
                            case LogSeverity.Debug:
                                _logger.LogTrace(logMessage.Exception, logMessage.Message);
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
                try
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
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred whilst setting bot displayed game");
                    throw;
                }
                
                await Task.Delay(60000);
            }
        }
    }
}