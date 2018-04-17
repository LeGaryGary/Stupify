using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stupify.Data.CustomCommandBuiltIns
{
    class DuplicateText: BuiltInCommand
    {
        public DuplicateText(IServiceProvider provider) : base(provider)
        {
            Tag = "$duplicateText";
            Execute = args =>
            {
                var repeatNumber = int.Parse(args[1]);
                if (repeatNumber > 50) throw new InvalidOperationException();
                var result = new List<string>();
                for (var i = 0; i < repeatNumber; i++)
                {
                    result.Add(args[0]);
                }

                var separator = bool.Parse(args[3]) ? "," : "";

                return Task.FromResult(string.Join(separator, result));
            };
        }
    }

    class DuplicateCommand: BuiltInCommand
    {
        public DuplicateCommand(IServiceProvider provider) : base(provider)
        {
            Tag = "$duplicateCommand";
            Execute = args =>
            {
                var repeatNumber = int.Parse(args[1]);
                if (repeatNumber > 50) throw new InvalidOperationException();
                var result = new List<string>();
                for (var i = 0; i < repeatNumber; i++)
                {
                    result.Add("$" + args[0]);
                }

                var separator = bool.Parse(args[2]) ? "," : "";

                return Task.FromResult(string.Join(separator, result));
            };
        }
    }
}
