using HB.FullStack.Common;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.ClientEntity
{
    public static class ClientEntityDefFactory
    {
        private static readonly ConcurrentDictionary<Type, ClientEntityDef> _clientEntityDefs = new ConcurrentDictionary<Type, ClientEntityDef>();

        public static ClientEntityDef? Get<T>() where T : Entity
        {
            Type type = typeof(T);

            if (_clientEntityDefs.TryGetValue(type, out ClientEntityDef? def))
            {
                return def;
            }

            //TODO: Use Source Generation
            ClientEntityAttribute? localDataAttribute = type.GetCustomAttribute<ClientEntityAttribute>(true);

            if (localDataAttribute == null)
            {
                return null;
            }

            ClientEntityDef newDef = new ClientEntityDef
            {
                ExpiryTime = TimeSpan.FromSeconds(localDataAttribute.ExpirySeconds),
                NeedLogined = localDataAttribute.NeedLogined,
                AllowOfflineRead = localDataAttribute.AllowOfflineRead,
                AllowOfflineWrite = localDataAttribute.AllowOfflineWrite
            };

            if (_clientEntityDefs.TryAdd(type, newDef))
            {
                return newDef;
            }

            return _clientEntityDefs[type];
        }

        public static void Register<T>(ClientEntityDef def) where T : Entity
        {
            _ = _clientEntityDefs.AddOrUpdate(typeof(T), def, (_, _) => def);
        }
    }
}
