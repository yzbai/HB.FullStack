using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.KVStore.Config;

namespace HB.FullStack.KVStore.Engine
{
    public interface IKVStoreEngine
    {
        void Initialize(KVStoreOptions options);

        /// <summary>
        /// 返回 modelJson - timestamp
        /// </summary>
        Task<IEnumerable<Tuple<byte[]?, long>>> GetAsync(string schemaName, string modelName, IEnumerable<string> modelKeys);

        /// <summary>
        /// 返回 modelJson - timestamp
        /// </summary>
        Task<IEnumerable<Tuple<byte[]?, long>>> GetAllAsync(string schemaName, string modelName);


        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task AddAsync(string schemaName, string modelName, IEnumerable<string> modelKeys, IEnumerable<byte[]?> models, long newTimestamp);


        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task UpdateAsync(string schemaName, string modelName, IEnumerable<string> modelKeys, IEnumerable<byte[]?> models, IEnumerable<long> timestamps, long newTimestamp);

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task DeleteAsync(string schemaName, string modelName, IEnumerable<string> modelKeys, IEnumerable<long> timestamps);

        
        Task<bool> DeleteAllAsync(string schemaName, string modelName);
    }
}
