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
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
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
        public static string CommandPrefix => Configuration["CommandPrefix"];
        public static ulong DeveloperRole => ulong.Parse(Configuration["DeveloperRole"]);

        public static string DataDirectory => Configuration["DataDirectory"];
        public static string UniverseName => Configuration["UniverseName"];

        public static bool UseSeq => bool.Parse(Configuration["Seq:Enabled"]);
        public static Uri SeqUrl => new Uri(Configuration["Seq:Url"]);
        public static string SeqKey => Configuration["Seq:ApiKey"];

        private static IServiceProvider _serviceProvider;


        public static IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider != null) return _serviceProvider;

                var collection = new ServiceCollection()
                    .AddSingleton(new LoggerFactory().AddSerilog())
                    .AddLogging()
                    .AddDbContext<BotContext>(options => options.UseSqlServer(DbConnectionString))
                    .AddSingleton<IDiscordClient>(sp => new DiscordSocketClient(new DiscordSocketConfig{AlwaysDownloadUsers = true, MessageCacheSize = 100}))
                    .AddSingleton<IMessageHandler, MessageHandler>()
                    .AddSingleton<IReactionHandler, SegmentEditReactionHandler>()
                    .AddSingleton(sp =>
                    {
                        var commandService = new CommandService();
                        commandService.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();
                        return commandService;
                    })
                    .AddSingleton<ClientManager>()
                    .AddTransient<TicTacZapController>()
                    .AddSingleton<GameState>()
                    .AddSingleton<GameRunner>();

                ConfigureLogging();
                _serviceProvider = collection.BuildServiceProvider();
                return _serviceProvider;
            }
        }

        private static void ConfigureLogging()
        {
            var config = new LoggerConfiguration()
                    .WriteTo.LiterateConsole()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning);

            if (UseSeq) config = config.WriteTo.Seq(SeqUrl.AbsoluteUri, apiKey: SeqKey);

            config = Debug ? config.MinimumLevel.Verbose() : config.MinimumLevel.Information();

            Log.Logger = config.CreateLogger();
        }
    }
}