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

        Task<IEnumerable<Tuple<string?, int>>> EntityGetAsync(string storeName, string entityName, IEnumerable<string> entityKeys);

        
        Task<IEnumerable<Tuple<string?, int>>> EntityGetAllAsync(string storeName, string entityName);

        Task EntityAddAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons);


        Task EntityUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons, IEnumerable<int> entityVersions);

        Task EntityDeleteAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions);

        Task<bool> EntityDeleteAllAsync(string storeName, string entityName);

        [Obsolete("不做Version检查，所以淘汰")]
        Task<IEnumerable<int>> EntityAddOrUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons);
    }
}
