using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace HB.Infrastructure.MongoDB
{
    public interface IMongoDatabaseManager
    {
        IMongoDatabase GetDatabase<T>() where T : class, new();

        IMongoCollection<T> GetCollection<T>() where T:class, new();
    }
}
