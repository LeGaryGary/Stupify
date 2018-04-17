using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stupify.Data.Repositories;
using TwitchApi;

namespace StupifyConsoleApp.Client
{
    public class ClientManager
    {
        private readonly ILogger<ClientManager> _logger;
        private readonly IDiscordClient _client;
        private readonly TwitchClient _twitchClient;
        private bool _ready;

        public ClientManager(IMessageHandler messageHandler, SegmentEditReactionHandler segmentEditHandler, ILogger<ClientManager> logger, IDiscordClient client, TwitchClient twitchClient)
        {
            _logger = logger;
            _client = client;
            _twitchClient = twitchClient;

            switch (_client)
            {
                case DiscordSocketClient discordSocketClient:
                    discordSocketClient.MessageReceived += messageHandler.HandleAsync;
                    discordSocketClient.ReactionAdded += segmentEditHandler.HandleAsync;
                    discordSocketClient.Ready += () =>
                    {
                        _ready = true;
                        return Task.CompletedTask;
                    };
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

        public async Task StartAsync()
        {
            switch (_client)
            {
                case DiscordSocketClient discordSocketClient:
                    await discordSocketClient.LoginAsync(TokenType.Bot, Config.DiscordBotUserToken).ConfigureAwait(false);
                    break;
                case DiscordShardedClient discordShardedClient:
                    await discordShardedClient.LoginAsync(TokenType.Bot, Config.DiscordBotUserToken).ConfigureAwait(false);
                    break;
            }
            
            await _client.StartAsync().ConfigureAwait(false);
            while (true)
            {
                try
                {
                    await SetGamePresenceAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred whilst setting bot displayed game");
                }

                try
                {
                    await UpdateTwichStatusChannelsAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred whilst updating twitch channels");
                }
                
                await Task.Delay(60000).ConfigureAwait(false);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private async Task UpdateTwichStatusChannelsAsync()
        {
            var twitchRepository = Config.ServiceProvider.GetService<ITwitchRepository>();

            var twitchUpdateChannels =
                await twitchRepository.AllTwitchUpdateChannelsAsync().ConfigureAwait(false);
            foreach (var twitchChannel in twitchUpdateChannels)
            {
                var isStreaming = await _twitchClient.IsStreamingAsync(twitchChannel.TwitchLoginName)
                    .ConfigureAwait(false);
                if (twitchChannel.LastStatus == isStreaming) continue;

                await twitchRepository
                    .UpdateLastStatusAsync(twitchChannel.GuildId, twitchChannel.TwitchLoginName, isStreaming)
                    .ConfigureAwait(false);

                if (!isStreaming) continue;

                while (!_ready)
                {
                    await Task.Delay(500).ConfigureAwait(false);
                }

                var guild = await _client.GetGuildAsync((ulong) twitchChannel.GuildId).ConfigureAwait(false);
                var channel = await guild.GetChannelAsync((ulong) twitchChannel.UpdateChannel)
                    .ConfigureAwait(false);

                var embed = await GetTwitchStreamEmbedAsync(twitchChannel.TwitchLoginName).ConfigureAwait(false);

                (channel as ITextChannel)?.SendMessageAsync(string.Empty, embed: embed);
            }
        }

        private async Task<Embed> GetTwitchStreamEmbedAsync(string twitchLoginName)
        {
            var stream = await _twitchClient.GetStreamAsync(twitchLoginName).ConfigureAwait(false);
            var game = await _twitchClient.GetGameTitleAsync(stream.GameId).ConfigureAwait(false);
            var embed = new EmbedBuilder
            {
                Title = $"{twitchLoginName} is now streaming {game.Name}!",
                ImageUrl = stream.ThumbnailUrl.Replace("{width}", "640").Replace("{height}", "480"),
                Url = $"https://www.twitch.tv/{twitchLoginName}",
                ThumbnailUrl = game.BoxArtUrl.Replace("{width}", "128").Replace("{height}", "128"),
                Color = new Color(100, 65, 164)
            };
            embed.AddField("Title", stream.Title, true)
                .AddField("Viewers", stream.ViewerCount);

            return embed.Build();
        }

        private async Task SetGamePresenceAsync()
        {
            switch (_client)
            {
                case DiscordShardedClient discordShardedClient:
                    await discordShardedClient
                        .SetGameAsync($"{Config.CommandPrefix} help | Servers: {discordShardedClient.Guilds.Count}")
                        .ConfigureAwait(false);
                    break;
                case DiscordSocketClient discordSocketClient:
                    await discordSocketClient
                        .SetGameAsync($"{Config.CommandPrefix} help | Servers: {discordSocketClient.Guilds.Count}")
                        .ConfigureAwait(false);
                    break;
            }
        }
    }
}