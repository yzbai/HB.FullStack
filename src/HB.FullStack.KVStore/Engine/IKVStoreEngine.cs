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
        
        Task<IEnumerable<Tuple<string?, int>>> EntityGetAsync(string storeName, string entityName, IEnumerable<string> entityKeys);

        
        Task<IEnumerable<Tuple<string?, int>>> EntityGetAllAsync(string storeName, string entityName);

        
        Task EntityAddAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons);

        
        Task EntityUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons, IEnumerable<int> entityVersions);

        
        Task EntityDeleteAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions);

        
        Task<bool> EntityDeleteAllAsync(string storeName, string entityName);


    }
}
