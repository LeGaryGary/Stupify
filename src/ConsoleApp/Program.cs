using System;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var startTask = ClientManager.Start();
            var tickTacZapTask = TicTacZapController.Run();
            while (!startTask.IsCompleted)
            {
                switch (Console.ReadLine()?.ToLower())
                {
                    case "help":
                        Console.WriteLine("Commands are: " +
                                          "start " +
                                          "stop");
                        break;
                    case "start":
                        ClientManager.Client.StartAsync().GetAwaiter().GetResult();
                        break;
                    case "stop":
                        ClientManager.Client.StopAsync().GetAwaiter().GetResult();
                        break;
                    default:
                        Console.WriteLine("Unknown command, try help");
                        break;
                }
            }
        }
    }
}