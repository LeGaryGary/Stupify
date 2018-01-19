﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace StupifyConsoleApp.Client
{
    public static class ClientManager
    {
        public static Logger Logger { get; }
        public static DiscordSocketClient Client { get; }
        public static CommandService Commands { get; }

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

        public static async Task Start()
        {
            await Client.LoginAsync(TokenType.Bot, Config.DiscordBotUserToken);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        public static async Task LogAsync(LogMessage message)
        {
            await Logger.Log(message.ToString(), false);
        }

        public static async Task LogAsync(string message, bool requireDebug = false)
        {
            await Logger.Log(DateTime.Now.ToString("T")+" "+message, requireDebug);
        }
    }
}