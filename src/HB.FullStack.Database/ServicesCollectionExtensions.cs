#nullable enable

using HB.FullStack.Database;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            //public
            services.AddSingleton<ITransaction, DefaultTransaction>();
            services.AddSingleton<IDatabase, DefaultDatabase>();
            services.AddSingleton<IDatabaseReader>(sp => sp.GetRequiredService<IDatabase>());
            services.AddSingleton<IDatabaseWriter>(sp => sp.GetRequiredService<IDatabase>());

            return services;
        }
    }
}