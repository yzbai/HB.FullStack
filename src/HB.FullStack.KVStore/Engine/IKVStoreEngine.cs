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
        /// <param name="modelName"></param>
        /// <param name="modelKeys"></param>
        /// <returns></returns>
        
        Task<IEnumerable<Tuple<string?, int>>> ModelGetAsync(string storeName, string modelName, IEnumerable<string> modelKeys);

        
        Task<IEnumerable<Tuple<string?, int>>> ModelGetAllAsync(string storeName, string modelName);

        
        Task ModelAddAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<string?> modelJsons);

        
        Task ModelUpdateAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<string?> modelJsons, IEnumerable<int> modelVersions);

        
        Task ModelDeleteAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<int> modelVersions);

        
        Task<bool> ModelDeleteAllAsync(string storeName, string modelName);


    }
}
