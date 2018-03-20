using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.DataModels;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp
{
    public static class Config
    {
        private static readonly IConfigurationRoot Configuration;

        static Config()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets("Stupify");
            Configuration = builder.Build();
        }

        public static string DbConnectionString => Configuration["DbConnectionString"];
        public static string DiscordBotUserToken => Configuration["DiscordBotUserToken"];
        public static bool Debug => bool.Parse(Configuration["Debug"]);
        public static string LoggingDirectory => Configuration["LoggingDirectory"];
        public static string CommandPrefix => Configuration["CommandPrefix"];
        public static ulong DeveloperRole => ulong.Parse(Configuration["DeveloperRole"]);

        public static string DataDirectory => Configuration["DataDirectory"];
        public static string UniverseName => Configuration["UniverseName"];

        public static IServiceProvider ServiceProvider
        {
            get
            {
                var collection = new ServiceCollection()
                    .AddLogging()
                    .AddDbContext<BotContext>(options => options.UseSqlServer(DbConnectionString))
                    .AddSingleton<IDiscordClient>(sp => new DiscordSocketClient(new DiscordSocketConfig{AlwaysDownloadUsers = true}))
                    .AddSingleton<IMessageHandler, MessageHandler>()
                    .AddSingleton(sp =>
                    {
                        var commandService = new CommandService();
                        commandService.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();
                        return commandService;
                    })
                    .AddSingleton<ClientManager>()
                    .AddSingleton<TicTacZapController>();
                return collection.BuildServiceProvider();
            }
        }
    }
}