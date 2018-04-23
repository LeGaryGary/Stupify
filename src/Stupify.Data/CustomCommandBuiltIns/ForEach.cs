using System;
using System.Linq;
using System.Threading.Tasks;

namespace Stupify.Data.CustomCommandBuiltIns
{
    class ForEach: BuiltInCommand
    {
        public ForEach(IServiceProvider provider) : base(provider)
        {
            Tag = "$applyToAll";
            Execute = args =>
            {
                var result = args.Skip(1).Select(arg => "$" + args[0].Replace("@", arg)).ToList();
                return Task.FromResult(string.Join("", result));
            };
        }
    }
}
