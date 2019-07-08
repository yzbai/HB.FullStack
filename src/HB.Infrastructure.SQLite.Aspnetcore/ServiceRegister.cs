using HB.Framework.Database;
using HB.Infrastructure.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddSQLite(this IServiceCollection services, IConfiguration configuration)
        {
            SQLiteOptions options = new SQLiteOptions();
            configuration.Bind(options);

            AddSQLiteInternal(services, options);

            return services;
        }
        public static IServiceCollection AddSQLite(this IServiceCollection services, Action<SQLiteOptions> databaseEngineOptionsSetup)
        {
            SQLiteOptions options = new SQLiteOptions();
            databaseEngineOptionsSetup(options);

            AddSQLiteInternal(services, options);

            return services;
        }

        private static void AddSQLiteInternal(IServiceCollection services, SQLiteOptions options)
        {
            SQLiteBuilder engineBuilder = new SQLiteBuilder()
                            .SetSQLiteOptions(options)
                            .Build();

            IDatabase database = new DatabaseBuilder()
                .SetDatabaseSettings(engineBuilder.DatabaseSettings)
                .SetDatabaseEngine(engineBuilder.DatabaseEngine)
                .Build();

            services.AddSingleton(database);
        }

    }
}
