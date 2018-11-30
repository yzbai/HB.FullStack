using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.DocumentStore;
using MongoDB.Driver;

namespace HB.Infrastructure.MongoDB
{
    public interface IMongoDB
    {
        IMongoCollection<T> GetCollection<T>() where T: DocumentStoreEntity, new();
    }
}
