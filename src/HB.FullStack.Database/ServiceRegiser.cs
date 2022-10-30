

using System;

using HB.FullStack.Common.Models;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class DatabaseEngineBuilder : IDatabaseEngineBuilder
    {
        public IServiceCollection Services { get; }

        public DatabaseEngineBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }

    public interface IDatabaseEngineBuilder
    {
        IServiceCollection Services { get; }
    }

    public static class ServiceRegiser
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, Action<DatabaseOptions> optionsAction, Action<IDatabaseEngineBuilder> configureDatabaseEngine)
        {
            services
                .Configure(optionsAction)
                .AddDatabaseCore();

            configureDatabaseEngine(new DatabaseEngineBuilder(services));

            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, Action<IDatabaseEngineBuilder> configureDatabaseEngine)
        {
            services
                .Configure<DatabaseOptions>(configuration)
                .AddDatabaseCore();

            configureDatabaseEngine(new DatabaseEngineBuilder(services));

            return services;
        }

        private static IServiceCollection AddDatabaseCore(this IServiceCollection services)
        {
            //public
            services.AddSingleton<IDbModelDefFactory, DbModelDefFactory>();
            services.AddSingleton<ISQLExpressionVisitor, SQLExpressionVisitor>();
            services.AddSingleton<IDbCommandBuilder, DbCommandBuilder>();
            services.AddSingleton<ITransaction, DefaultTransaction>();
            services.AddSingleton<IDatabase, DefaultDatabase>();
            services.AddSingleton<IDatabaseReader>(sp => sp.GetRequiredService<IDatabase>());
            services.AddSingleton<IDatabaseWriter>(sp => sp.GetRequiredService<IDatabase>());

            //IModelDefProvider
            services.AddSingleton(typeof(IModelDefProvider), sp => sp.GetRequiredService(typeof(IDbModelDefFactory)));

            return services;
        }
    }
}