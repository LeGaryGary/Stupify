using System;
using System.Reflection;
using System.Threading.Tasks;
using ConsoleApp.Client;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        private ClientManager _clientManager;

        public async Task StartAsync()
        {
            try
            {
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
