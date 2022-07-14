

using HB.FullStack.Database;
using HB.FullStack.Database.DatabaseModels;
using HB.FullStack.Database.SQL;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            //public
            services.AddSingleton<IDBModelDefFactory, DBModelDefFactory>();
            services.AddSingleton<ISQLExpressionVisitor, SQLExpressionVisitor>(); 
            services.AddSingleton<IDbCommandBuilder, DbCommandBuilder>();
            services.AddSingleton<ITransaction, DefaultTransaction>();
            services.AddSingleton<IDatabase, DefaultDatabase>();
            services.AddSingleton<IDatabaseReader>(sp => sp.GetRequiredService<IDatabase>());
            services.AddSingleton<IDatabaseWriter>(sp => sp.GetRequiredService<IDatabase>());

            return services;
        }
    }
}