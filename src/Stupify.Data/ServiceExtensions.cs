using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stupify.Data.FileSystem;
using Stupify.Data.Repositories;
using Stupify.Data.SQL;

namespace Stupify.Data
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSqlDatabase(this IServiceCollection collection, string dbConnectionString)
        {
            collection.AddSingleton(sp => new DbContextOptionsBuilder<BotContext>().UseSqlServer(dbConnectionString).Options);
            collection.AddTransient(sp => new BotContext(sp.GetService<DbContextOptions<BotContext>>()));

            return collection;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection collection, string dataDirectory)
        {
            collection.TryAddSingleton<Random>();
            collection.AddSingleton(sp => new FileSegments(dataDirectory, sp.GetService<Random>()))
                .AddSingleton(sp => new Inventories(dataDirectory))
                .AddSingleton(sp =>
                {
                    var ctrl = new UniverseController(dataDirectory, "Alpha");
                    ctrl.Start().GetAwaiter().GetResult();
                    return ctrl;
                })
                .AddSingleton(sp => new SegmentTemplates(dataDirectory))

                .AddTransient<IUserRepository, UserRepository>()
                .AddTransient<ISegmentRepository, SegmentRepository>()
                .AddTransient<IInventoryRepository, InventoryRepository>()
                .AddTransient<IUniverseRepository, UniverseRepository>()
                .AddTransient<ITemplateRepository, TemplateRepository>();

            return collection;
        }
    }
}
