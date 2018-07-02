using HB.Framework.Database;
using HB.Framework.Database.Entity;
using HB.Framework.Database.SQL;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using HB.Framework.Database.Engine;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<DatabaseOptions>(configuration);

            services.AddSingleton<IDatabaseEntityDefFactory, DefaultDatabaseEntityDefFactory>();
            services.AddSingleton<IDatabaseEntityMapper, DefaultDatabaseEntityMapper>();
            services.AddSingleton<ISQLBuilder, SQLBuilder>();

            services.AddSingleton<IDatabase, DefaultDatabase>();

            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, Action<DatabaseOptions> databaseSchemaOptionsSetup)
        {
            services.AddOptions();

            services.Configure(databaseSchemaOptionsSetup);

            services.AddSingleton<IDatabaseEntityDefFactory, DefaultDatabaseEntityDefFactory>();
            services.AddSingleton<IDatabaseEntityMapper, DefaultDatabaseEntityMapper>();
            services.AddSingleton<ISQLBuilder, SQLBuilder>();

            services.AddSingleton<IDatabase, DefaultDatabase>();

            return services;
        }
    }
}
