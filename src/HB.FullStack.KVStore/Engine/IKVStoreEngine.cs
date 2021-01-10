using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.KVStore.Engine
{
    public interface IKVStoreEngine
    {
        KVStoreSettings Settings { get; }

        string FirstDefaultInstanceName { get; }

        void Close();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="entityName"></param>
        /// <param name="entityKeys"></param>
        /// <returns></returns>
        /// <exception cref="KVStoreException"></exception>
        Task<IEnumerable<Tuple<string?, int>>> EntityGetAsync(string storeName, string entityName, IEnumerable<string> entityKeys);

        /// <exception cref="KVStoreException"></exception>
        Task<IEnumerable<Tuple<string?, int>>> EntityGetAllAsync(string storeName, string entityName);

        /// <exception cref="KVStoreException"></exception>
        Task EntityAddAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons);

        /// <exception cref="KVStoreException"></exception>
        Task EntityUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons, IEnumerable<int> entityVersions);

        /// <exception cref="KVStoreException"></exception>
        Task EntityDeleteAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions);

        /// <exception cref="KVStoreException"></exception>
        Task<bool> EntityDeleteAllAsync(string storeName, string entityName);


    }
}
