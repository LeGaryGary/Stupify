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
        private static void Main(string[] args)
        {
            var startTask = Config.ServiceProvider.GetService<ClientManager>().Start();
            var tickTacZapTask = Config.ServiceProvider.GetService<TicTacZapController>().Run();
            Task.Delay(-1).Wait();
        }
    }
}