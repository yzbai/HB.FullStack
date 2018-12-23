using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Engine
{
    public interface IKVStoreEngineAsync
    {
        Task<string>                EntityGetAsync(string storeName, int storeIndex, string entityName, string entityKey);
        Task<IEnumerable<string>>   EntityGetAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys);
        Task<IEnumerable<string>>   EntityGetAllAsync(string storeName, int storeIndex, string entityName);

        Task<KVStoreResult>         EntityAddAsync(string storeName, int storeIndex, string entityName, string entityKey, string entityJson);
        Task<KVStoreResult>         EntityAddAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons);

        Task<KVStoreResult>         EntityUpdateAsync(string storeName, int storeIndex, string entityName, string entityKey, string entityJson, int entityVersion);
        Task<KVStoreResult>         EntityUpdateAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons, IEnumerable<int> entityVersions);

        Task<KVStoreResult>         EntityDeleteAsync(string storeName, int storeIndex, string entityName, string entityKey, int entityVersion);
        Task<KVStoreResult>         EntityDeleteAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions);
        Task<KVStoreResult>         EntityDeleteAllAsync(string storeName, int storeIndex, string entityName);
        
    }
}
