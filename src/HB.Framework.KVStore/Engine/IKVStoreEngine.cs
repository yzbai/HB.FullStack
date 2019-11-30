using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.KVStore.Engine
{
    public interface IKVStoreEngine : IKVStoreEngineAsync
    {
        KVStoreSettings Settings { get; }

        string FirstDefaultInstanceName { get; }

        void Close();

        string EntityGet(string storeName, string entityName, string entityKey);
        IEnumerable<string> EntityGet(string storeName, string entityName, IEnumerable<string> entityKeys);
        IEnumerable<string> EntityGetAll(string storeName, string entityName);

        void EntityAdd(string storeName, string entityName, string entityKey, string entityJson);
        void EntityAdd(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons);

        void EntityUpdate(string storeName, string entityName, string entityKey, string entityJson, int entityVersion);
        void EntityUpdate(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons, IEnumerable<int> entityVersions);

        void EntityDelete(string storeName, string entityName, string entityKey, int entityVersion);
        void EntityDelete(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions);
        bool EntityDeleteAll(string storeName, string entityName);

    }
}
