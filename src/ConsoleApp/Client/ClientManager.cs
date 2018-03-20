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
        private readonly DiscordSocketClient _client;

        public ClientManager(IMessageHandler messageHandler, ILogger<ClientManager> logger, DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += messageHandler.Handle;
            _client.Log += logMessage =>
            {
                switch (logMessage.Severity)
                {
                    case LogSeverity.Critical:
                        logger.LogCritical(logMessage.Message);
                        break;
                    case LogSeverity.Error:
                        logger.LogError(logMessage.Message);
                        break;
                    case LogSeverity.Warning:
                        logger.LogWarning(logMessage.Message);
                        break;
                    case LogSeverity.Info:
                        logger.LogInformation(logMessage.Message);
                        break;
                    case LogSeverity.Verbose:
                        logger.LogDebug(logMessage.Message);
                        break;
                    case LogSeverity.Debug:
                        logger.LogTrace(logMessage.Message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return Task.CompletedTask;
            };
        }

        public async Task Start()
        {
            await _client.LoginAsync(TokenType.Bot, Config.DiscordBotUserToken);
            await _client.StartAsync();
            while (true)
            {
                await _client.SetGameAsync($"{Config.CommandPrefix}help | Servers: {_client.Guilds.Count}");
                await Task.Delay(60000);
            }
        }
    }
}