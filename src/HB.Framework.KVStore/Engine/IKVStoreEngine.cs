using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Engine
{
    public interface IKVStoreEngine : IKVStoreEngineAsync
    {
        byte[]              EntityGet(string storeName, int storeIndex, string entityName, string entityKey);
        IEnumerable<byte[]> EntityGet(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys);      
        IEnumerable<byte[]> EntityGetAll(string storeName, int storeIndex, string entityName);

        KVStoreResult       EntityAdd(string storeName, int storeIndex, string entityName, string entityKey, byte[] entityValue);
        KVStoreResult       EntityAdd(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<byte[]> entityValues);

        KVStoreResult       EntityUpdate(string storeName, int storeIndex, string entityName, string entityKey, byte[] entityValue, int entityVersion);
        KVStoreResult       EntityUpdate(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<byte[]> entityValues, IEnumerable<int> entityVersions);

        KVStoreResult       EntityDelete(string storeName, int storeIndex, string entityName, string entityKey, int entityVersion);
        KVStoreResult       EntityDelete(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions);
        KVStoreResult       EntityDeleteAll(string storeName, int storeIndex, string entityName);
        

    }
}
