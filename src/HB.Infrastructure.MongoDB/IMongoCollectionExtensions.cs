using HB.Framework.DocumentStore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.Infrastructure.MongoDB
{
    public static class IMongoCollectionExtensions
    {

        public static async Task<T> GetByGuidAsync<T>(this IMongoCollection<T> collection, string id) where T : DocumentStoreEntity, new()
        {
            IAsyncCursor<T> cursor = await collection.FindAsync(t => t.Guid == id).ConfigureAwait(false);

            return cursor.FirstOrDefault();
        }

        /// <summary>
        /// TODO:坑，得到的结果，不是按传入id顺序排列的. 解决效率问题
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static async Task<IList<T>> GetByGuidsAsync<T>(this IMongoCollection<T> collection, IEnumerable<string> ids) where T : DocumentStoreEntity, new()
        {
            IAsyncCursor<T> cursor = await collection.FindAsync(Builders<T>.Filter.In(t => t.Guid, ids)).ConfigureAwait(false);

            IList<T> list = cursor.ToEnumerable().ToList();

            // tidy the order
            List<T> newList = new List<T>();

            foreach(string id in ids)
            {
                T tt = list.FirstOrDefault(t => t.Guid == id);
                if (tt != null)
                {
                    newList.Add(tt);
                }
            }

            return newList;
        }

        public static T GetByGuid<T>(this IMongoCollection<T> collection, string id) where T : DocumentStoreEntity, new()
        {
            return collection.Find(t => t.Guid == id).FirstOrDefault();
        }

        /// <summary>
        /// TODO:坑，得到的结果，不是按传入id顺序排列的. 解决效率问题
        /// </summary>
        public static IList<T> GetByGuids<T>(this IMongoCollection<T> collection, IEnumerable<string> ids) where T : DocumentStoreEntity, new()
        {
            IFindFluent<T, T> fluent = collection.Find(Builders<T>.Filter.In(t => t.Guid, ids));

            IList<T> list = fluent.ToEnumerable().ToList();

            List<T> newList = new List<T>();

            foreach (string id in ids)
            {
                T tt = list.FirstOrDefault(t => t.Guid == id);

                if (tt != null)
                {
                    newList.Add(tt);
                }
            }

            return newList;
        }
    }
}
