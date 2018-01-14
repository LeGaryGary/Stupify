using System;
using System.IO;
using System.Threading.Tasks;

namespace StupifyConsoleApp
{
    public class Logger
    {
        private readonly string _filePath;

        private string Path => Directory.GetCurrentDirectory() + "\\Logs\\" + _filePath;

        public Logger(string directory)
        {
            Directory.CreateDirectory(
                System.IO.Path.Combine(
                    Directory.GetCurrentDirectory(),
                    directory));
            _filePath = directory + "\\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
        }

        public async Task Log(string message)
        {
            await Console.Out.WriteLineAsync(message).ConfigureAwait(false);
            await File.AppendAllTextAsync(Path, message + Environment.NewLine).ConfigureAwait(false);
        }
    }
}