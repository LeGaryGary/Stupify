using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ConsoleApp.Client;
using Microsoft.Extensions.Configuration;

namespace ConsoleApp
{
    internal class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        private ClientManager _clientManager;

        private static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddUserSecrets("Stupify");
                Configuration = builder.Build();

                _clientManager = new ClientManager();
                await _clientManager.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
    }
}
