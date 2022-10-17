﻿

using HB.FullStack.Common.Models;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            //public
            services.AddSingleton<IDbModelDefFactory, DbModelDefFactory>();
            services.AddSingleton<ISQLExpressionVisitor, SQLExpressionVisitor>();
            services.AddSingleton<IDbCommandBuilder, DbCommandBuilder>();
            services.AddSingleton<ITransaction, DefaultTransaction>();
            services.AddSingleton<IDatabase, DefaultDatabase>();
            services.AddSingleton<IDatabaseReader>(sp => sp.GetRequiredService<IDatabase>());
            services.AddSingleton<IDatabaseWriter>(sp => sp.GetRequiredService<IDatabase>());

            //IModelDefProvider
            services.AddSingleton(typeof(IModelDefProvider), sp => sp.GetRequiredService(typeof(IDbModelDefFactory)));

            return services;
        }
    }
}