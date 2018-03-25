using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NYoutubeDL;
using Serilog;
using Serilog.Events;
using Stupify.Data;
using StupifyConsoleApp.Client;
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
        public static string YoutubeApiKey => Configuration["YoutubeApiKey"];
        public static bool Debug => bool.Parse(Configuration["Debug"]);
        public static string CommandPrefix => Configuration["CommandPrefix"];
        public static ulong DeveloperRole => ulong.Parse(Configuration["DeveloperRole"]);
        public static bool DeleteCommands => bool.Parse(Configuration["DeleteCommands"]);

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
                    .AddSqlDatabase(DbConnectionString)
                    .AddRepositories(DataDirectory)
                    .AddSingleton<IDiscordClient>(sp => new DiscordSocketClient(new DiscordSocketConfig{AlwaysDownloadUsers = true, MessageCacheSize = 1000}))
                    .AddSingleton<IMessageHandler, MessageHandler>()
                    .AddSingleton<IReactionHandler, SegmentEditReactionHandler>()
                    .AddSingleton(sp =>
                    {
                        var commandService = new CommandService();
                        commandService.AddModulesAsync(Assembly.GetEntryAssembly()).GetAwaiter().GetResult();
                        var logger = sp.GetService<ILogger<MessageHandler>>();
                        commandService.Log += message =>
                        {
                            logger.LogError(message.Exception, "Unhandled exception in command service");
                            return Task.CompletedTask;
                        };
                        return commandService;
                    })
                    .AddTransient(sp => new YouTubeService(new BaseClientService.Initializer
                        {
                            ApiKey = YoutubeApiKey
                        }))
                    .AddSingleton<ClientManager>()
                    .AddTransient<TicTacZapController>()
                    .AddSingleton<SegmentEditReactionHandler>()
                    .AddSingleton<GameState>()
                    .AddTransient<GameRunner>()
                    .AddSingleton<AudioService>()
                    .AddTransient(sp => new YoutubeDL($"{Directory.GetCurrentDirectory()}/youtube-dl.exe"));

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