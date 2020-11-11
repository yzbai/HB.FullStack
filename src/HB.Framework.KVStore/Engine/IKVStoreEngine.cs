using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Engine
{
    public interface IKVStoreEngine
    {
        KVStoreSettings Settings { get; }

        string FirstDefaultInstanceName { get; }

        void Close();

        /// <exception cref="KVStoreException"></exception>
        Task<string> EntityGetAsync(string storeName, string entityName, string entityKey);

        /// <exception cref="KVStoreException"></exception>
        Task<IEnumerable<string>> EntityGetAsync(string storeName, string entityName, IEnumerable<string> entityKeys);

        /// <exception cref="KVStoreException"></exception>
        Task<IEnumerable<string>> EntityGetAllAsync(string storeName, string entityName);

        Task EntityAddAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons);

        Task<IEnumerable<int>> EntityAddOrUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons);

        Task EntityUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons, IEnumerable<int> entityVersions);

        Task EntityDeleteAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions);

        Task<bool> EntityDeleteAllAsync(string storeName, string entityName);

    }
}
