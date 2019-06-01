﻿using HB.Framework.Database;
using HB.Framework.Database.Entity;
using HB.Framework.Database.SQL;
using System;
using Microsoft.Extensions.Configuration;
using HB.Framework.Database.Transaction;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<DatabaseOptions>(configuration);

            AddDatabase(services);

            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, Action<DatabaseOptions> databaseSchemaOptionsSetup)
        {
            services.AddOptions();

            services.Configure(databaseSchemaOptionsSetup);

            AddDatabase(services);

            return services;
        }

        private static void AddDatabase(IServiceCollection services)
        {
            //internal 
            services.AddSingleton<IDatabaseTypeConverterFactory, DatabaseTypeConverterFactory>();
            services.AddSingleton<IDatabaseEntityDefFactory, DefaultDatabaseEntityDefFactory>();
            services.AddSingleton<IDatabaseEntityMapper, DefaultDatabaseEntityMapper>();
            services.AddSingleton<ISQLBuilder, SQLBuilder>();

            //public interface
            services.AddSingleton<ITransaction, Transaction>();
            services.AddSingleton<IDatabase, DefaultDatabase>();

        }
    }
}
