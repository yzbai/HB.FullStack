using HB.Framework.DocumentStore;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddDocumentStore(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DocumentStoreOptions>(configuration);

            return services;
        }
    }
}
