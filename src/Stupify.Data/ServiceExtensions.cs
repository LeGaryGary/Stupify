using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stupify.Data.FileSystem;
using Stupify.Data.SQL;

namespace Stupify.Data
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSqlDatabase(this IServiceCollection collection, string dbConnectionString)
        {
            collection.AddDbContext<BotContext>(options => options.UseSqlServer(dbConnectionString));

            return collection;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection collection, string dataDirectory)
        {
            collection.TryAddSingleton<Random>();
            collection.AddSingleton(sp => new FileSegments(dataDirectory, sp.GetService<Random>()));

            collection.AddTransient<ISegmentRepository, SegmentRepository>();

            return collection;
        }
    }
}
