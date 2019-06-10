using HB.Framework.Database.Engine;
using HB.Infrastructure.MySQL;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddMySQLEngine(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<MySQLEngineOptions>(configuration);

            services.AddSingleton<IDatabaseEngine, MySQLEngine>();

            return services;
        }

        public static IServiceCollection AddMySQLEngine(this IServiceCollection services, Action<MySQLEngineOptions> databaseEngineOptionsSetup)
        {
            services.AddOptions();

            services.Configure(databaseEngineOptionsSetup);

            services.AddSingleton<IDatabaseEngine, MySQLEngine>();

            return services;
        }
    }
}
