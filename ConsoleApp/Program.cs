using System;
using System.Threading;
using System.Threading.Tasks;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var clientTask = ClientManager.Start();
            while (!clientTask.GetAwaiter().IsCompleted)
            {
                Console.ReadLine();
            }
        }
    }
}