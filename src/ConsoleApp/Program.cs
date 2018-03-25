using System;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Stupify.Data.Repositories;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp
{
    internal class Program
    {
        private static void Main() => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            var universeRepository= Config.ServiceProvider.GetService<IUniverseRepository>();
            var segmentRepository= Config.ServiceProvider.GetService<ISegmentRepository>();
            
            foreach (var segment in universeRepository.UniverseSegments())
            {
                if (!segmentRepository.Exists(segment).GetAwaiter().GetResult())
                    universeRepository.DeleteSegmentAsync(segment).GetAwaiter().GetResult();
            }

            var startTask = Config.ServiceProvider.GetService<ClientManager>().Start();
            while (true)
            {
                await Config.ServiceProvider.GetService<GameRunner>().Run();
            }
        }
    }
}