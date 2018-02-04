using System;
using System.IO;
using Discord;
using Microsoft.Extensions.Configuration;
using Neo4j.Driver.V1;

namespace StupifyConsoleApp
{
    public static class Config
    {
        private static readonly IConfigurationRoot Configuration;

        public static string DbConnectionString => Configuration["DbConnectionString"];
        public static string DiscordBotUserToken => Configuration["DiscordBotUserToken"];
        public static bool Debug => bool.Parse(Configuration["Debug"]);
        public static string LoggingDirectory => Configuration["LoggingDirectory"];
        public static string CommandPrefix => Configuration["CommandPrefix"]+" ";

        public static bool Neo4JMessageHandlerEnabled => bool.Parse(Configuration["MessageAnalysis:Enabled"]);
        public static Uri Neo4JUri => new Uri(Configuration["MessageAnalysis:Neo4JUri"]);
        public static IAuthToken Neo4JAuth => AuthTokens.Basic(Configuration["MessageAnalysis:Neo4JUser"], Configuration["MessageAnalysis:Neo4JPassword"]);

        static Config()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets("Stupify");
            Configuration = builder.Build();
        }
    }
}