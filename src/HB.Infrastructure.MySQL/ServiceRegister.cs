using HB.FullStack.Database.Engine;
using HB.Infrastructure.MySQL;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddMySQL(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddOptions();

            services.Configure<MySQLOptions>(configuration);

            services.AddSingleton<IDatabaseEngine, MySQLEngine>();

            services.AddDatabase();

            return services;
        }

        public static IServiceCollection AddMySQL(this IServiceCollection services, Action<MySQLOptions> databaseEngineOptionsSetup)
        {
            //services.AddOptions();

            services.Configure(databaseEngineOptionsSetup);

            services.AddSingleton<IDatabaseEngine, MySQLEngine>();

            services.AddDatabase();

            return services;
        }
    }
}