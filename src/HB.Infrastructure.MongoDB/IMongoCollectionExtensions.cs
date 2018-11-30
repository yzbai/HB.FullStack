using HB.Framework.DocumentStore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Infrastructure.MongoDB
{
    public static class IMongoCollectionExtensions
    {

        public static async Task<T> GetByIdAsync<T>(this IMongoCollection<T> collection, string id) where T : DocumentStoreEntity, new()
        {
            IAsyncCursor<T> cursor = await collection.FindAsync(t => t.Id == id);

            return cursor.FirstOrDefault();
        }

        public static async Task<IEnumerable<T>> GetByIdsAsync<T>(this IMongoCollection<T> collection, IEnumerable<string> ids) where T : DocumentStoreEntity, new()
        {
            IAsyncCursor<T> cursor = await collection.FindAsync(Builders<T>.Filter.In(t => t.Id, ids));

            return cursor.ToEnumerable();
        }

        public static T GetById<T>(this IMongoCollection<T> collection, string id) where T : DocumentStoreEntity, new()
        {
            return collection.Find(t => t.Id == id).FirstOrDefault();
        }

        public static IEnumerable<T> GetByIds<T>(this IMongoCollection<T> collection, IEnumerable<string> ids) where T : DocumentStoreEntity, new()
        {
            IFindFluent<T, T> fluent = collection.Find(Builders<T>.Filter.In(t => t.Id, ids));

            return fluent.ToEnumerable();
        }

    }
}
