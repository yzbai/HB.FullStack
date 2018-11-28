using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.MongoDB
{
    public class MongoDatabaseManager : IMongoDatabaseManager
    {
        private MongoDBOptions _options;
        private ILogger _logger;
        private IMongoClient _client;

        private IDictionary<Type, IMongoDatabase> _dbDict;

        private IDictionary<Type, object> _collectionDict;

        public MongoDatabaseManager(IOptions<MongoDBOptions> options, ILogger<MongoDatabaseManager> logger, IMongoClient mongoClient)
        {
            _options = options.Value;
            _logger = logger;
            _client = mongoClient;

            _dbDict = new Dictionary<Type, IMongoDatabase>();
            _collectionDict = new Dictionary<Type, object>();
        }

        public IMongoCollection<T> GetCollection<T>() where T : class, new()
        {
            Type type = typeof(T);

            if (_collectionDict.ContainsKey(type))
            {
                return _collectionDict[type] as IMongoCollection<T>;
            }

            IMongoDatabase db = GetDatabase<T>();

            if (db == null)
            {
                return null;
            }

            IMongoCollection<T> collection = db.GetCollection<T>(nameof(T));

            if (collection != null)
            {
                _collectionDict[type] = collection;
            }

            return collection;
        }

        public IMongoDatabase GetDatabase<T>() where T : class, new()
        {
            Type key = typeof(T);

            if (_dbDict.ContainsKey(key))
            {
                return _dbDict[key];
            }

            string dbName = _options.GetDatabaseName(key);

            if (string.IsNullOrEmpty(dbName))
            {
                return null;
            }

            IMongoDatabase database = _client.GetDatabase(dbName);

            if (database != null)
            {
                _dbDict[key] = database;
            }

            return database;
        }
    }
}
