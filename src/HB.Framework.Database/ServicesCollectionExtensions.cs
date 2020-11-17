#nullable enable

using HB.Framework.Database;
using HB.Framework.Database.Entities;
using HB.Framework.Database.SQL;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesCollectionExtensions
    {
        /// <summary>
        /// Called by Database Infrastructure. ex: MySQL, SQLite
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseTypeConverterFactory, DatabaseTypeConverterFactory>();
            services.AddSingleton<IDatabaseEntityDefFactory, DefaultDatabaseEntityDefFactory>();
            services.AddSingleton<IDatabaseEntityMapper, DefaultDatabaseEntityMapper>();
            services.AddSingleton<ISQLBuilder, SQLBuilder>();
            services.AddSingleton<ITransaction, DefaultTransaction>();
            services.AddSingleton<IDatabase, DefaultDatabase>();

            return services;
        }
    }
}
