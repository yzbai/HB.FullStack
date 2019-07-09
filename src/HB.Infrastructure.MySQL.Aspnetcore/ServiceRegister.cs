using HB.Framework.Database;
using HB.Framework.Database.Engine;
using HB.Infrastructure.MySQL;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddMySQL(this IServiceCollection services, IConfiguration configuration)
        {
            MySQLOptions options = new MySQLOptions();
            configuration.Bind(options);

            AddMySQLInternal(services, options);

            return services;
        }
        public static IServiceCollection AddMySQL(this IServiceCollection services, Action<MySQLOptions> databaseEngineOptionsSetup)
        {
            MySQLOptions options = new MySQLOptions();
            databaseEngineOptionsSetup(options);

            AddMySQLInternal(services, options);

            return services;
        }

        private static void AddMySQLInternal(IServiceCollection services, MySQLOptions options)
        {
            IDatabase database = new DatabaseBuilder(new MySQLBuilder(options).Build()).Build();

            services.AddSingleton(database);
        }

    }
}
