using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TwitchApi;

namespace Stupify.Data.CustomCommandBuiltIns
{
    internal class StreamLive : BuiltInCommand
    {
        public StreamLive(IServiceProvider provider) : base(provider)
        {
            Tag = "$areTheyLive";
            Execute = async args => (await provider.GetService<TwitchClient>().IsStreamingAsync(args[0]).ConfigureAwait(false)).ToString().ToLower();
        }
    }
}
