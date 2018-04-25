using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace StupifyConsoleApp.Client
{
    // This service requires that your bot is being run by a daemon that handles
    // Exit Code 1 (or any exit code) as a restart.
    //
    // If you do not have your bot setup to run in a daemon, this service will just
    // terminate the process and the bot will not restart.
    // 
    // Links to daemons:
    // [Powershell (Windows+Unix)] https://gitlab.com/snippets/21444
    // [Bash (Unix)] https://stackoverflow.com/a/697064
    public class ReliabilityService
    {
        // --- Begin Configuration Section ---
        // How long should we wait on the client to reconnect before resetting?
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        // Should we attempt to reset the client? Set this to false if your client is still locking up.
        private bool AttemptReset = true;

        // Change log levels if desired:
        private const LogSeverity Debug = LogSeverity.Debug;
        private const LogSeverity Info = LogSeverity.Info;
        private const LogSeverity Critical = LogSeverity.Critical;

        // --- End Configuration Section ---

        private readonly DiscordSocketClient _discord;
        private readonly ILogger<ReliabilityService> _logger;
        private CancellationTokenSource _cts;        

        public ReliabilityService(DiscordSocketClient discord, bool attemptReset, ILogger<ReliabilityService> logger)
        {
            _cts = new CancellationTokenSource();
            _discord = discord;
            _logger = logger;
        }

        public void Attach()
        {
            _discord.Connected += ConnectedAsync;
            _discord.Disconnected += DisconnectedAsync;
        }

        private Task ConnectedAsync()
        {
            // Cancel all previous state checks and reset the CancelToken - client is back online
            _logger.LogDebug("Client reconnected, resetting cancel tokens...");
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            _logger.LogDebug("Client reconnected, cancel tokens reset.");

            return Task.CompletedTask;
        }

        private Task DisconnectedAsync(Exception e)
        {
            // Check the state after <timeout> to see if we reconnected
            _logger.LogError(e, "Client disconnected, starting timeout task...");
            _ = Task.Delay(Timeout, _cts.Token).ContinueWith(async _ =>
            {
                _logger.LogDebug("Timeout expired, checking client state...");
                await CheckStateAsync().ConfigureAwait(false);
            });

            return Task.CompletedTask;
        }

        private async Task CheckStateAsync()
        {
            // Client reconnected, no need to reset
            if (_discord.ConnectionState == ConnectionState.Connected)
            {
                _logger.LogDebug("State came back okay");
                return;
            }

            // Client unable to reconnect, try to restart
            if (AttemptReset)
            {
                _logger.LogInformation("Attempting to reset the client");

                var timeout = Task.Delay(Timeout);
                var connect = _discord.StartAsync();
                var task = await Task.WhenAny(timeout, connect).ConfigureAwait(false);

                if (task == timeout)
                {
                    _logger.LogCritical("Client reset timed out (task deadlocked?), killing process");
                    FailFast();
                }
                else if (connect.IsFaulted)
                {
                    _logger.LogCritical("Client reset faulted, killing process", connect.Exception);
                    FailFast();
                }
                else if (connect.IsCompletedSuccessfully)
                    _logger.LogInformation("Client reset successfully!");
                return;
            }

            _logger.LogCritical("Client did not reconnect in time, killing process");
            FailFast();
        }

        private static void FailFast() => Environment.Exit(1);
    }
}