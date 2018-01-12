using System.IO;
using Microsoft.Extensions.Configuration;

namespace StupifyConsoleApp
{
    public static class Config
    {
        private static readonly IConfigurationRoot Configuration;

        public static string DbConnectionString => Configuration["DbConnectionString"];
        public static string DiscordBotUserToken => Configuration["DiscordBotUserToken"];

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