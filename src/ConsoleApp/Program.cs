using System;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp
{
    internal class Program
    {
        private static void Main() => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            var startTask = Config.ServiceProvider.GetService<ClientManager>().Start();
            while (true)
            {
                await Config.ServiceProvider.GetService<GameRunner>().Run();
            }
        }
    }
}