using HB.Infrastructure.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoDBServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDB(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<MongoDBOptions>(configuration);

            serviceCollection.AddSingleton<IMongoDB, DefaultMongoDB>();

            return serviceCollection;
        }
    }
}
