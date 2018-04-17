using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Stupify.Data.CustomCommandBuiltIns
{
    internal class Add : BuiltInCommand
    {
        public Add(IServiceProvider provider) : base(provider)
        {
            Tag = "$add";
            Execute = args => Task.FromResult((decimal.Parse(args[0]) + decimal.Parse(args[1])).ToString(CultureInfo.InvariantCulture));
        }
    }
}
