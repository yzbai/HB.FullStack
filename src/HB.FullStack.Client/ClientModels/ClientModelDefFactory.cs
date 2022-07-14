using HB.FullStack.Common;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.ClientModels
{
    public static class ClientModelDefFactory
    {
        private static readonly ConcurrentDictionary<Type, ClientModelDef> _clientModelDefs = new ConcurrentDictionary<Type, ClientModelDef>();

        public static ClientModelDef? Get<T>() where T : IModel
        {
            Type type = typeof(T);

            if (_clientModelDefs.TryGetValue(type, out ClientModelDef? def))
            {
                return def;
            }

            //TODO: Use Source Generation
            ClientModelAttribute? localDataAttribute = type.GetCustomAttribute<ClientModelAttribute>(true);

            if (localDataAttribute == null)
            {
                return null;
            }

            ClientModelDef newDef = new ClientModelDef
            {
                ExpiryTime = TimeSpan.FromSeconds(localDataAttribute.ExpirySeconds),
                NeedLogined = localDataAttribute.NeedLogined,
                AllowOfflineRead = localDataAttribute.AllowOfflineRead,
                AllowOfflineWrite = localDataAttribute.AllowOfflineWrite
            };

            if (_clientModelDefs.TryAdd(type, newDef))
            {
                return newDef;
            }

            return _clientModelDefs[type];
        }

        public static void Register<T>(ClientModelDef def) where T : IModel
        {
            _ = _clientModelDefs.AddOrUpdate(typeof(T), def, (_, _) => def);
        }
    }
}
