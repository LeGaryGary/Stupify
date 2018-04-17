using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stupify.Data.CustomCommandBuiltIns
{
    class Multiply : BuiltInCommand
    {
        public Multiply(IServiceProvider provider) : base(provider)
        {
            Tag = "$multiply";
            Execute = args =>
            {
                var result = decimal.Parse(args[0]);
                result = args.Skip(1).Aggregate(result, (current, arg) => current * decimal.Parse(arg));
                return Task.FromResult(result.ToString(CultureInfo.InvariantCulture));
            };
        }
    }
}
