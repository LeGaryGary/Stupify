using System.Threading.Tasks;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Program().StartAsync().GetAwaiter().GetResult();
        }

        public async Task StartAsync()
        {
            await ClientManager.Start();
        }
    }
}