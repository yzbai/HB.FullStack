using HB.Framework.Common.Entities;
using HB.Framework.KVStore.Engine;
using HB.Framework.KVStore.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HB.Framework.KVStore.Entities
{
    internal class DefaultKVStoreModelDefFactory : IKVStoreEntityDefFactory
    {
        private readonly object _lockObj = new object();
        private readonly IKVStoreEngine _kvStoreEngine;
        private readonly KVStoreSettings _settings;
        private readonly IDictionary<string, KVStoreEntitySchema> _typeSchemaDict = new Dictionary<string, KVStoreEntitySchema>();
        private readonly ConcurrentDictionary<Type, KVStoreEntityDef> _defDict = new ConcurrentDictionary<Type, KVStoreEntityDef>();

        public DefaultKVStoreModelDefFactory(IKVStoreEngine kVStoreEngine)
        {
            _kvStoreEngine = kVStoreEngine;
            _settings = _kvStoreEngine.Settings;

            IEnumerable<Type> allEntityTypes;

            if (_settings.AssembliesIncludeEntity.IsNullOrEmpty())
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(t => t.IsSubclassOf(typeof(Entity)));
            }
            else
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(_settings.AssembliesIncludeEntity, t => t.IsSubclassOf(typeof(Entity)));
            }

            _typeSchemaDict = ConstructeSchemaDict(allEntityTypes);
        }

        private IDictionary<string, KVStoreEntitySchema> ConstructeSchemaDict(IEnumerable<Type> allEntityTypes)
        {
            IDictionary<string, KVStoreEntitySchema> filedDict = _settings.KVStoreEntities.ToDictionary(t => t.EntityTypeFullName);
            IDictionary<string, KVStoreEntitySchema> resultDict = new Dictionary<string, KVStoreEntitySchema>();

            allEntityTypes.ForEach(type =>
            {
                KVStoreEntitySchemaAttribute attribute = type.GetCustomAttribute<KVStoreEntitySchemaAttribute>();

                filedDict.TryGetValue(type.FullName, out KVStoreEntitySchema fileConfigured);

                string? instanceName = null;

                if (attribute != null)
                {
                    instanceName = attribute.InstanceName.IsNullOrEmpty() ? _kvStoreEngine.FirstDefaultInstanceName : attribute.InstanceName!;
                }

                if (fileConfigured != null)
                {
                    instanceName = fileConfigured.InstanceName;
                }

                if (instanceName.IsNullOrEmpty())
                {
                    instanceName = _kvStoreEngine.FirstDefaultInstanceName;
                }

                KVStoreEntitySchema entitySchema = new KVStoreEntitySchema
                {
                    EntityTypeFullName = type.FullName,
                    InstanceName = instanceName!
                };

                resultDict.Add(type.FullName, entitySchema);
            });

            return resultDict;
        }

        /// <summary>
        /// GetDef
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public KVStoreEntityDef GetDef<T>()
        {
            return GetDef(typeof(T));
        }

        /// <summary>
        /// GetDef
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public KVStoreEntityDef GetDef(Type type)
        {
            if (!_defDict.ContainsKey(type))
            {
                lock (_lockObj)
                {
                    if (!_defDict.ContainsKey(type))
                    {
                        _defDict[type] = CreateEntityDef(type);
                    }
                }
            }

            return _defDict[type];
        }

        /// <summary>
        /// CreateEntityDef
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        private KVStoreEntityDef CreateEntityDef(Type type)
        {
            if (!_typeSchemaDict.TryGetValue(type.FullName, out KVStoreEntitySchema storeEntitySchema))
            {
                throw new KVStoreException(ErrorCode.KVStoreNoEntitySchemaFound, type.FullName);
            }

            KVStoreEntityDef entityDef = new KVStoreEntityDef(storeEntitySchema.InstanceName, type);

            #region Handle Key Properties

            PropertyInfo[] properties = type.GetTypeInfo().GetProperties();

            foreach (PropertyInfo info in properties)
            {
                KVStoreKeyAttribute keyAttr = info.GetCustomAttribute<KVStoreKeyAttribute>();

                if (keyAttr != null)
                {
                    entityDef.KeyPropertyInfos.Add(keyAttr.Order, info);
                }
            }

            if (!entityDef.KeyPropertyInfos.Any())
            {
                throw new KVStoreException(Resources.LackKVStoreKeyAttributeErrorMessage);
            }

            #endregion

            return entityDef;
        }
    }
}
