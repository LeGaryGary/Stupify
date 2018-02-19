using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Neo4j.Driver.V1;

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

        public static bool Neo4JMessageHandlerEnabled => bool.Parse(Configuration["MessageAnalysis:Enabled"]);
        public static Uri Neo4JUri => new Uri(Configuration["MessageAnalysis:Neo4JUri"]);

        public static IAuthToken Neo4JAuth => AuthTokens.Basic(Configuration["MessageAnalysis:Neo4JUser"],
            Configuration["MessageAnalysis:Neo4JPassword"]);
    }
}