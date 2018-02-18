using System;
using System.IO;
using System.Threading.Tasks;

namespace StupifyConsoleApp
{
    public class Logger
    {
        private readonly string _relativeLogFilePath;

        private string Path => Directory.GetCurrentDirectory()+"\\"+_relativeLogFilePath;

        public Logger(string directory)
        {
            Directory.CreateDirectory(
                System.IO.Path.Combine(
                    Directory.GetCurrentDirectory(),
                    directory));
            _relativeLogFilePath = directory + "\\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
        }

        public async Task Log(string message, bool requireDebug)
        {
            if (!requireDebug || Config.Debug)
            {
                var consoleWrite = Console.Out.WriteLineAsync(message);
                var writeToLogFile = File.AppendAllTextAsync(Path, message + Environment.NewLine);
                await Task.WhenAll(consoleWrite, writeToLogFile);
            }
        }
    }
}