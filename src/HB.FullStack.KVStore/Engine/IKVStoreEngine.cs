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
        Task<IEnumerable<Tuple<byte[]?, long>>> GetAsync(string storeName, string modelName, IEnumerable<string> modelKeys);

        /// <summary>
        /// 返回 modelJson - timestamp
        /// </summary>
        Task<IEnumerable<Tuple<byte[]?, long>>> GetAllAsync(string storeName, string modelName);


        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task AddAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<byte[]?> models, long newTimestamp);


        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task UpdateAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<byte[]?> models, IEnumerable<long> timestamps, long newTimestamp);

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task DeleteAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<long> timestamps);

        
        Task<bool> DeleteAllAsync(string storeName, string modelName);


    }
}
