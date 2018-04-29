using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace StupifyConsoleApp.Client.CustomTypeReaders
{
    public class UriTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            try
            {
                var result = new Uri(input);
                return Task.FromResult(TypeReaderResult.FromSuccess(result));
            }
            catch
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a Uri."));
            }
        }
    }
}