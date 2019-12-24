using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Engine
{
    public interface IKVStoreEngine
    {
        KVStoreSettings Settings { get; }

        string FirstDefaultInstanceName { get; }

        void Close();

        Task<string> EntityGetAsync(string storeName, string entityName, string entityKey);
        Task<IEnumerable<string>> EntityGetAsync(string storeName, string entityName, IEnumerable<string> entityKeys);
        Task<IEnumerable<string>> EntityGetAllAsync(string storeName, string entityName);

        Task EntityAddAsync(string storeName, string entityName, string entityKey, string entityJson);
        Task EntityAddAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons);

        Task EntityUpdateAsync(string storeName, string entityName, string entityKey, string entityJson, int entityVersion);
        Task EntityUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons, IEnumerable<int> entityVersions);

        Task EntityDeleteAsync(string storeName, string entityName, string entityKey, int entityVersion);
        Task EntityDeleteAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions);
        Task<bool> EntityDeleteAllAsync(string storeName, string entityName);

    }
}
