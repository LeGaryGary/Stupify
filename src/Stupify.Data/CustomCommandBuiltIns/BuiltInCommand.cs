using System;
using System.Threading.Tasks;

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
        public Func<string[], Task<string>> Execute { get; protected set; }
    }
}
