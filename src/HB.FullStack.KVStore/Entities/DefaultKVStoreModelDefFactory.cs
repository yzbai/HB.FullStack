﻿using HB.FullStack.Common.Entities;
using HB.FullStack.KVStore.Engine;
using HB.FullStack.KVStore.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HB.FullStack.KVStore.Entities
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
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(kvstoreEntityTypeCondition);
            }
            else
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(_settings.AssembliesIncludeEntity, kvstoreEntityTypeCondition);
            }

            _typeSchemaDict = ConstructeSchemaDict(allEntityTypes);

            static bool kvstoreEntityTypeCondition(Type t)
            {
                return t.IsSubclassOf(typeof(Entity)) && t.GetCustomAttribute<KVStoreEntityAttribute>() != null;
            }
        }

        private IDictionary<string, KVStoreEntitySchema> ConstructeSchemaDict(IEnumerable<Type> allEntityTypes)
        {
            IDictionary<string, KVStoreEntitySchema> filedDict = _settings.KVStoreEntities.ToDictionary(t => t.EntityTypeFullName);
            IDictionary<string, KVStoreEntitySchema> resultDict = new Dictionary<string, KVStoreEntitySchema>();

            allEntityTypes.ForEach(type =>
            {
                KVStoreEntityAttribute attribute = type.GetCustomAttribute<KVStoreEntityAttribute>();

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
        
        public KVStoreEntityDef GetDef<T>()
        {
            return GetDef(typeof(T));
        }

        /// <summary>
        /// GetDef
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        
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
        
        private KVStoreEntityDef CreateEntityDef(Type type)
        {
            if (!_typeSchemaDict.TryGetValue(type.FullName, out KVStoreEntitySchema storeEntitySchema))
            {
                throw new KVStoreException(ErrorCode.KVStoreNoEntitySchemaFound, type.FullName);
            }

            KVStoreEntityDef entityDef = new KVStoreEntityDef(storeEntitySchema.InstanceName, type);

            #region Handle Key Properties

            PropertyInfo[] properties = type.GetTypeInfo().GetProperties();

            PropertyInfo? backupKeyPropertyInfo = null;

            foreach (PropertyInfo info in properties)
            {
                KVStoreKeyAttribute keyAttr = info.GetCustomAttribute<KVStoreKeyAttribute>();

                if (keyAttr != null)
                {
                    entityDef.KeyPropertyInfos.Add(keyAttr.Order, info);
                }
                else if (info.GetCustomAttribute<KVStoreBackupKeyAttribute>() != null)
                {
                    backupKeyPropertyInfo = info;
                }
            }

            //如果KVStoreKey没有找到，就启用BackupKey

            if (!entityDef.KeyPropertyInfos.Any())
            {
                if (backupKeyPropertyInfo == null)
                {
                    throw new KVStoreException(Resources.LackKVStoreKeyAttributeErrorMessage);
                }

                entityDef.KeyPropertyInfos.Add(0, backupKeyPropertyInfo);
            }

            #endregion

            return entityDef;
        }
    }
}