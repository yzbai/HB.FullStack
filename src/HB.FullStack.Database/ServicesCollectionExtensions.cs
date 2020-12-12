#nullable enable

using HB.FullStack.Database;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.SQL;

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
            services.AddSingleton<IDatabaseEntityDefFactory, DefaultDatabaseEntityDefFactory>();
            services.AddSingleton<IDbCommandBuilder, DbCommandBuilder>();

            //public
            services.AddSingleton<ITransaction, DefaultTransaction>();
            services.AddSingleton<IDatabase, DefaultDatabase>();
            services.AddSingleton<IDatabaseReader>(sp => sp.GetRequiredService<IDatabase>());
            services.AddSingleton<IDatabaseWriter>(sp => sp.GetRequiredService<IDatabase>());

            return services;
        }
    }
}
