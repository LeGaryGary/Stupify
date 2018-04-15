using System;
using System.Collections.Generic;
using System.Text;

namespace Stupify.Data.CustomCommandBuiltIns
{
    internal abstract class BuiltInCommand
    {
        protected readonly IServiceProvider Provider;

        protected BuiltInCommand(IServiceProvider provider)
        {
            Provider = provider;
        }

        public string Tag { get; protected set; }
        public Func<string[], string> Execute { get; protected set; }
    }
}
