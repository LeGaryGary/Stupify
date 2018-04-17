using System;
using System.Threading.Tasks;

namespace Stupify.Data.CustomCommandBuiltIns
{
    internal class If : BuiltInCommand
    {
        public If(IServiceProvider provider) : base(provider)
        {
            Tag = "$if";
            Execute = args => Task.FromResult(args[0] == "true" ? args[1] : args[2]);
        }
    }
}
