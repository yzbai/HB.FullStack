using System;
using HB.Framework.Database.Engine;
using HB.Infrastructure.SQLite;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddSQLite(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<SQLiteOptions>(configuration);

            AddSQLiteCore(services);

            return services;
        }

        public static IServiceCollection AddSQLite(this IServiceCollection services, Action<SQLiteOptions> databaseEngineOptionsSetup)
        {
            services.AddOptions();

            services.Configure(databaseEngineOptionsSetup);

            AddSQLiteCore(services);

            return services;
        }

        private static void AddSQLiteCore(IServiceCollection services)
        {
            services.AddSingleton<IDatabaseEngine, SQLiteEngine>();

            services.AddDatabase();

            SQLitePCL.Batteries_V2.Init();
        }
    }
}