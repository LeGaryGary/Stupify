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

        public ClientManager()
        {
            Client = new DiscordSocketClient();
            Commands = new CommandService();

            Commands.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();

            Client.Log += Log;
            Client.MessageReceived += MessageHandler.HandleCommand;
        }

        public async Task Start()
        {
            await Client.LoginAsync(TokenType.Bot, "Mzk5OTk4Mjc1NTA5Mjg4OTYy.DTVWRw.7EkOvblXABuPRXfI-708nl8i7o8");
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
