using System;
using Microsoft.Extensions.DependencyInjection;

namespace Stupify.Data.CustomCommandBuiltIns
{
    internal class RandomInt : BuiltInCommand
    {
        public RandomInt(IServiceProvider provider) : base(provider)
        {
            Tag = "$randInt";
            Execute = args =>
            {
                var max = int.Parse(args[1]);
                var min = int.Parse(args[0]);
                return (Provider.GetService<Random>().Next(max - min + 1) + min).ToString();
            };
        }
    }

    internal class RandomChoice : BuiltInCommand
    {
        public RandomChoice(IServiceProvider provider) : base(provider)
        {
            Tag = "$choose";
            Execute = args =>
            {
                var random = Provider.GetService<Random>();
                var index = random.Next(args.Length);
                return args[index];
            };
        }
    }
}
