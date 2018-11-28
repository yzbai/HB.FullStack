using HB.Infrastructure.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoDBServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDB(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<MongoDBOptions>(configuration);

            MongoDBOptions options = new MongoDBOptions();
            configuration.Bind(options);

            serviceCollection.AddSingleton<IMongoClient>(new MongoClient(options.ConnectionString));

            serviceCollection.AddSingleton<IMongoDatabaseManager, MongoDatabaseManager>();

            return serviceCollection;
        }
    }
}
