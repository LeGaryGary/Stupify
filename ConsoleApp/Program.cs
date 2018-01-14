using System;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var startTask = ClientManager.Start();
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