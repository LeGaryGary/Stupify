using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
