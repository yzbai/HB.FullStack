using System;

using HB.FullStack.Common.Models;
using HB.FullStack.Database;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Implements;
using HB.FullStack.Database.SQL;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegiser
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, Action<DbOptions> optionsAction, Action<IDbEngineBuilder> configureDbEngineBuilder)
        {
            services.Configure(optionsAction);
            services.AddDatabaseCore();

            configureDbEngineBuilder(new DbEngineBuilder(services));

            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, Action<IDbEngineBuilder> configureDbEngineBuilder)
        {
            services.Configure<DbOptions>(configuration);
            services.AddDatabaseCore();

            configureDbEngineBuilder(new DbEngineBuilder(services));

            return services;
        }

        private static IServiceCollection AddDatabaseCore(this IServiceCollection services)
        {
            //public
            services.AddSingleton<IDbModelDefFactory, DbModelDefFactory>();
            services.AddSingleton<IDbConfigManager, DbConfigManager>();
            services.AddSingleton<ISQLExpressionVisitor, SQLExpressionVisitor>();
            services.AddSingleton<IDbCommandBuilder, DbCommandBuilder>();
            services.AddSingleton<ITransaction, DefaultTransaction>();
            services.AddSingleton<IDatabase, DefaultDatabase>();
            services.AddSingleton<IDbReader>(sp => sp.GetRequiredService<IDatabase>());
            services.AddSingleton<IDbWriter>(sp => sp.GetRequiredService<IDatabase>());

            //IModelDefProvider
            services.AddSingleton(typeof(IModelDefProvider), sp => sp.GetRequiredService(typeof(IDbModelDefFactory)));

            return services;
        }
    }
}