global using SchemaName = System.String;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HB.FullStack.Common.Models;
using HB.FullStack.KVStore.Config;

using Microsoft.Extensions.Options;

namespace HB.FullStack.KVStore.KVStoreModels
{
    internal class KVStoreModelDefFactory : IKVStoreModelDefFactory, IModelDefProvider
    {
        private readonly IDictionary<SchemaName, KVStoreSchema> _schemaDict = null!;
        private readonly IDictionary<Type, KVStoreModelDef> _modelDefDict;
        private readonly KVStoreSchema _defaultSchema;
        private readonly KVStoreOptions _options;

        public KVStoreModelDefFactory(IOptions<KVStoreOptions> options)
        {
            _options = options.Value;

            //schema
            _schemaDict = _options.KVStoreSchemas.ToDictionary(s => s.Name);

            //default Schema
            _defaultSchema = _options.KVStoreSchemas.FirstOrDefault(s => s.IsDefault) ?? _options.KVStoreSchemas[0];


            IEnumerable<Type> allModelTypes = _options.KVStoreModelAssemblies.IsNullOrEmpty()
                ? ReflectionUtil.GetAllTypeByCondition(kvstoreModelTypeCondition)
                : ReflectionUtil.GetAllTypeByCondition(_options.KVStoreModelAssemblies, kvstoreModelTypeCondition);

            _modelDefDict = ConstructeModelDefDict(allModelTypes);

            static bool kvstoreModelTypeCondition(Type t)
            {
                return t.IsClass && !t.IsAbstract && !t.IsGenericType && t.IsAssignableTo(typeof(IKVStoreModel));
            }

            IDictionary<Type, KVStoreModelDef> ConstructeModelDefDict(IEnumerable<Type> allModelTypes)
            {
                Dictionary<Type, KVStoreModelDef> resultModelDefDict = new Dictionary<Type, KVStoreModelDef>();
                Dictionary<string, KVStoreSchema> optionsTypeSchemaDict = new Dictionary<SchemaName, KVStoreSchema>();

                foreach (var schema in _options.KVStoreSchemas)
                {
                    foreach (var typeFullName in schema.ModelTypeFullNames)
                    {
                        optionsTypeSchemaDict[typeFullName] = schema;
                    }
                }

                foreach (var type in allModelTypes)
                {
                    KVStoreModelDef modelDef = new KVStoreModelDef(type);

                    //Schema
                    if (optionsTypeSchemaDict.TryGetValue(type.FullName!, out KVStoreSchema? optionSchema))
                    {
                        modelDef.KVStoreSchema = optionSchema;
                    }
                    else
                    {
                        KVStoreAttribute? attribute = type.GetCustomAttribute<KVStoreAttribute>(true);

                        if (attribute != null && attribute.SchemaName.IsNotNullOrEmpty())
                        {
                            modelDef.KVStoreSchema = _schemaDict[attribute.SchemaName];
                        }
                        else
                        {
                            modelDef.KVStoreSchema = _defaultSchema;
                        }
                    }

                    modelDef.OrderedKeyPropertyInfos = GetOrderedKeyPropertyInfos(type);

                    resultModelDefDict[type] = modelDef;
                }

                return resultModelDefDict;

                static IList<PropertyInfo> GetOrderedKeyPropertyInfos(Type type)
                {

                    PropertyInfo[] properties = type.GetTypeInfo().GetProperties();

                    Dictionary<int, PropertyInfo> keyPropertyInfos = new Dictionary<int, PropertyInfo>();

                    foreach (PropertyInfo info in properties)
                    {
                        KVStoreKeyAttribute? keyAttr = info.GetCustomAttribute<KVStoreKeyAttribute>(true);

                        if (keyAttr != null)
                        {
                            keyPropertyInfos.Add(keyAttr.Order, info);
                        }
                    }

                    //如果KVStoreKey没有找到，就启用BackupKey

                    if (!keyPropertyInfos.Any())
                    {
                        throw KVStoreExceptions.OptionsError($"{type.FullName} do not has a {nameof(KVStoreKeyAttribute)}.");
                    }

                    return keyPropertyInfos.OrderBy(p => p.Key).Select(p => p.Value).ToList();
                }
            }
        }


        public KVStoreModelDef GetDef<T>() where T:class, IKVStoreModel
        {
            return GetDef(typeof(T))!;
        }

        public KVStoreModelDef? GetDef(Type type)
        {
            if(_modelDefDict.TryGetValue(type, out var modelDef))
            {
                return modelDef;
            }

            return null;
        }

        #region IModelDefProvider Memebers

        ModelKind IModelDefProvider.ModelKind => ModelKind.KV;

        ModelDef? IModelDefProvider.GetModelDef(Type type) => GetDef(type);

        #endregion
    }
}
