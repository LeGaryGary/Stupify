using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

            var _ = Config.ServiceProvider.GetService<ClientManager>().StartAsync();
            while (true)
            {
                await Config.ServiceProvider.GetService<GameRunner>().Run().ConfigureAwait(false);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}