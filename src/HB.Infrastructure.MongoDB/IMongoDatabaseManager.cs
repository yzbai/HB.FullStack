using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace HB.Infrastructure.MongoDB
{
    public interface IMongoManager
    {
        IMongoCollection<T> GetCollection<T>() where T:class, new();
    }
}
