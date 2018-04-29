using System;
using System.Threading.Tasks;

namespace Stupify.Data.CustomCommandBuiltIns
{
    internal class Equals : BuiltInCommand
    {
        public Equals(IServiceProvider provider) : base(provider)
        {
            Tag = "$equals";
            Execute = args => Task.FromResult((args[0] == args[1]).ToString().ToLower());
        }
    }
}
