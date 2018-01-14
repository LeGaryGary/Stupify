using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace StupifyConsoleApp.Client
{
    internal static class ClientManager
    {
        public static Logger Logger { get; set; }
        public static DiscordSocketClient Client { get; set; }
        public static CommandService Commands { get; set; }
        public static bool IsReady { get; private set; }

        static ClientManager()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true
            });
            Commands = new CommandService();
            Logger = new Logger(Config.LoggingDirectory);

            Commands.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();

            Client.Log += Log;
            Client.MessageReceived += MessageHandler.Handle;
            Client.Ready += Ready;
        }

        public static async Task Start()
        {
            await Client.LoginAsync(TokenType.Bot, Config.DiscordBotUserToken);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        private static Task Ready()
        {
            IsReady = true;
            return Task.CompletedTask;
        }

        public static async Task Log(LogMessage message)
        {
            await Logger.Log(message.ToString(), false).ConfigureAwait(false);
        }

        public static async Task Log(string message, bool requireDebug = false)
        {
            await Logger.Log(message, requireDebug).ConfigureAwait(false);
        }
    }
}