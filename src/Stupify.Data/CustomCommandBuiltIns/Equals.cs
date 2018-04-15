using System;

namespace Stupify.Data.CustomCommandBuiltIns
{
    internal class Equals : BuiltInCommand
    {
        public Equals(IServiceProvider provider) : base(provider)
        {
            Tag = "$equals";
            Execute = args => (args[0] == args[1]).ToString().ToLower();
        }
    }
}
