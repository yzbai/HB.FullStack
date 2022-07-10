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
        /// 返回 modelJson - timestamp
        /// </summary>
        Task<IEnumerable<Tuple<string?, long>>> ModelGetAsync(string storeName, string modelName, IEnumerable<string> modelKeys);

        /// <summary>
        /// 返回 modelJson - timestamp
        /// </summary>
        Task<IEnumerable<Tuple<string?, long>>> ModelGetAllAsync(string storeName, string modelName);

        
        Task ModelAddAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<string?> modelJsons, long newTimestamp);

        
        Task ModelUpdateAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<string?> modelJsons, IEnumerable<long> modelTimestamps, long newTimestamp);

        
        Task ModelDeleteAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<long> modelTimestamps);

        
        Task<bool> ModelDeleteAllAsync(string storeName, string modelName);


    }
}
