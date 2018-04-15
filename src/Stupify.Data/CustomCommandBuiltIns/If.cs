using System;

namespace Stupify.Data.CustomCommandBuiltIns
{
    internal class If : BuiltInCommand
    {
        public If(IServiceProvider provider) : base(provider)
        {
            Tag = "$if";
            Execute = args => args[0] == "true" ? args[1] : args[2];
        }
    }
}
