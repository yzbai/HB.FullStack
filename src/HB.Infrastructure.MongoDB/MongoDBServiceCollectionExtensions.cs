using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HB.Infrastructure.MongoDB
{
    public static class MongoDBServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDB(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            return null;
        }
    }
}
