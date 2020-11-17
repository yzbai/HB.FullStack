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
            ThrowIf.Null(databaseEngineOptionsSetup, nameof(databaseEngineOptionsSetup))(options);

            AddSQLiteInternal(services, options);

            return services;
        }

        private static void AddSQLiteInternal(IServiceCollection services, SQLiteOptions options)
        {
            IDatabase database = new DatabaseBuilder(new SQLiteBuilder(options).Build()).Build();

            services.AddSingleton(database);
        }

    }
}
