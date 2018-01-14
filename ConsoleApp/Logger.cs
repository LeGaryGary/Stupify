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
            if (!requireDebug)
            {
                await Console.Out.WriteLineAsync(message).ConfigureAwait(false);
                await File.AppendAllTextAsync(Path, message + Environment.NewLine).ConfigureAwait(false);
            }
            else if (Config.Debug)
            {
                message = "Debug: " + message;
                await Console.Out.WriteLineAsync(message).ConfigureAwait(false);
                await File.AppendAllTextAsync(Path, message + Environment.NewLine).ConfigureAwait(false);
            }
        }
    }
}