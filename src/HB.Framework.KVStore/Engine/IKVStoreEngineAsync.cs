using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Engine
{
    public interface IKVStoreEngineAsync : IDisposable
    {
        Task<byte[]>                EntityGetAsync(string storeName, int storeIndex, string entityName, string entityKey);
        Task<IEnumerable<byte[]>>   EntityGetAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys);
        Task<IEnumerable<byte[]>>   EntityGetAllAsync(string storeName, int storeIndex, string entityName);

        Task<KVStoreResult>         EntityAddAsync(string storeName, int storeIndex, string entityName, string entityKey, byte[] entityValue);
        Task<KVStoreResult>         EntityAddAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<byte[]> entityValues);

        Task<KVStoreResult>         EntityUpdateAsync(string storeName, int storeIndex, string entityName, string entityKey, byte[] entityValue, int entityVersion);
        Task<KVStoreResult>         EntityUpdateAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<byte[]> entityValues, IEnumerable<int> entityVersions);

        Task<KVStoreResult>         EntityDeleteAsync(string storeName, int storeIndex, string entityName, string entityKey, int entityVersion);
        Task<KVStoreResult>         EntityDeleteAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions);
        Task<KVStoreResult>         EntityDeleteAllAsync(string storeName, int storeIndex, string entityName);
        
    }
}
