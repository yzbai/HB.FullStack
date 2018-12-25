using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Engine
{
    public interface IKVStoreEngine : IKVStoreEngineAsync
    {
        string              EntityGet(string storeName, string entityName, string entityKey);
        IEnumerable<string> EntityGet(string storeName, string entityName, IEnumerable<string> entityKeys);      
        IEnumerable<string> EntityGetAll(string storeName, string entityName);

        KVStoreResult       EntityAdd(string storeName, string entityName, string entityKey, string entityJson);
        KVStoreResult       EntityAdd(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons);

        KVStoreResult       EntityUpdate(string storeName, string entityName, string entityKey, string entityJson, int entityVersion);
        KVStoreResult       EntityUpdate(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons, IEnumerable<int> entityVersions);

        KVStoreResult       EntityDelete(string storeName, string entityName, string entityKey, int entityVersion);
        KVStoreResult       EntityDelete(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions);
        KVStoreResult       EntityDeleteAll(string storeName, string entityName);
        
    }
}
