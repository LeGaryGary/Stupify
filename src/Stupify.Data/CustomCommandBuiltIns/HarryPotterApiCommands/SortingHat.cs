using System;
using Microsoft.Extensions.DependencyInjection;

namespace Stupify.Data.CustomCommandBuiltIns.HarryPotterApiCommands
{
    class SortingHat : BuiltInCommand
    {
        public SortingHat(IServiceProvider provider) : base(provider)
        {
            Tag = "$sortingHat";
            Execute = args => provider.GetService<HarryPotterApiClient>().SortingHatAsync();
        }
    }
}
