using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ConsoleApp.Client
{
    internal class ClientManager
    {
        public static DiscordSocketClient Client { get; set; }
        public static CommandService Commands { get; set; }
        private static string _discordBotUserToken;

        public ClientManager(string discordBotUserToken)
        {
            Client = new DiscordSocketClient();
            Commands = new CommandService();
            _discordBotUserToken = discordBotUserToken;

            Commands.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();

            Client.Log += Log;
            Client.MessageReceived += MessageHandler.HandleCommand;
        }

        public async Task Start()
        {
            await Client.LoginAsync(TokenType.Bot, _discordBotUserToken);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        private static Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}
