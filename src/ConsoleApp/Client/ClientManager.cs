using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace StupifyConsoleApp.Client
{
    public static class ClientManager
    {
        static ClientManager()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true
            });
            Commands = new CommandService();
            Logger = new Logger(Config.LoggingDirectory);

            Commands.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();

            Client.Log += LogAsync;
            Client.MessageReceived += MessageHandler.Handle;
        }

        public static DiscordSocketClient Client { get; }

        public static CommandService Commands { get; }

        private static Logger Logger { get; }

        public static async Task Start()
        {
            await Client.LoginAsync(TokenType.Bot, Config.DiscordBotUserToken);
            await Client.StartAsync();
            await SetStatus();
        }

        public static async Task LogAsync(string message, bool requireDebug = false)
        {
            await Logger.Log(DateTime.Now.ToString("T") + " " + message, requireDebug);
        }

        private static async Task LogAsync(LogMessage message)
        {
            await Logger.Log(message.ToString(), false);
        }

        private static async Task SetStatus()
        {
            while (true)
            {
                await Client.SetGameAsync($"{Config.CommandPrefix}help | Servers: {Client.Guilds.Count}");
                await Task.Delay(60000);
            }
        }
    }
}