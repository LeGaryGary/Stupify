using System;
using System.Linq;
using System.Threading.Tasks;

namespace Stupify.Data.CustomCommandBuiltIns
{
    class Contains : BuiltInCommand
    {
        public Contains(IServiceProvider provider) : base(provider)
        {
            Tag = "$contains";
            Execute = args => Task.FromResult(args.Skip(1).Contains(args[0]).ToString().ToLower());
        }
    }
}
