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
            collection.AddSingleton(sp => new FileSegments(dataDirectory, sp.GetService<Random>()))
                .AddSingleton(sp => new Inventories(dataDirectory))
                .AddSingleton(sp =>
                {
                    var ctrl = new UniverseController(dataDirectory, "Alpha");
                    ctrl.StartAsync().GetAwaiter().GetResult();

                    return ctrl;
                })
                .AddSingleton(sp => new SegmentTemplates(dataDirectory));

            

            return collection.AddRepositories();
        }

        public static IServiceCollection AddRepositories(this IServiceCollection collection)
        {
            collection.TryAddSingleton<Random>();

            collection.AddTransient<IUserRepository, UserRepository>()
                .AddTransient<ISegmentRepository, SegmentRepository>()
                .AddTransient<IInventoryRepository, InventoryRepository>()
                .AddTransient<IUniverseRepository, UniverseRepository>()
                .AddTransient<ITemplateRepository, TemplateRepository>()
                .AddTransient<IQuoteRepository, QuoteRepository>()
                .AddTransient<IStoryRepository, StoryRepository>()
                .AddTransient<ITwitchRepository, TwitchRepository>()
                .AddTransient<ICustomCommandRepository, CustomCommandRepository>()
                .AddTransient<ISettingsRepository, SettingsRepository>();

            return collection;
        }
    }
}
