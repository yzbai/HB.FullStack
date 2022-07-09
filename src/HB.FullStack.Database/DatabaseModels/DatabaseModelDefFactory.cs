using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HB.FullStack.Common;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database.DatabaseModels
{
    internal class DatabaseModelDefFactory : IDatabaseModelDefFactory
    {
        /// <summary>
        /// 这里不用ConcurrentDictionary。是因为在初始化时，就已经ConstructModelDefs，后续只有read，没有write
        /// </summary>
        private readonly IDictionary<Type, DatabaseModelDef> _defDict = new Dictionary<Type, DatabaseModelDef>();

        public DatabaseModelDefFactory(IDatabaseEngine databaseEngine)
        {
            DatabaseCommonSettings databaseSettings = databaseEngine.DatabaseSettings;

            IEnumerable<Type> allModelTypes;

            if (databaseSettings.Assemblies.IsNullOrEmpty())
            {
                allModelTypes = ReflectUtil.GetAllTypeByCondition(modelTypeCondition);
            }
            else
            {
                allModelTypes = ReflectUtil.GetAllTypeByCondition(databaseSettings.Assemblies, modelTypeCondition);
            }

            IDictionary<string, DatabaseModelSetting> modelSettingDict = ConstructeSettingDict(databaseSettings, databaseEngine, allModelTypes);

            ConstructModelDefs(allModelTypes, databaseEngine.EngineType, modelSettingDict);

            static bool modelTypeCondition(Type t)
            {
                return t.IsSubclassOf(typeof(DatabaseModel)) && !t.IsAbstract;
            }
        }

        private void ConstructModelDefs(IEnumerable<Type> allModelTypes, EngineType engineType, IDictionary<string, DatabaseModelSetting> modelSettingDict)
        {
            foreach (var t in allModelTypes)
            {
                _defDict[t] = CreateModelDef(t, engineType, modelSettingDict);
            }
        }

        private static IDictionary<string, DatabaseModelSetting> ConstructeSettingDict(DatabaseCommonSettings databaseSettings, IDatabaseEngine databaseEngine, IEnumerable<Type> allModelTypes)
        {
            IDictionary<string, DatabaseModelSetting> fileConfiguredDict = databaseSettings.ModelSettings.ToDictionary(t => t.ModelTypeFullName);

            IDictionary<string, DatabaseModelSetting> resusltModelSchemaDict = new Dictionary<string, DatabaseModelSetting>();

            foreach (Type type in allModelTypes)
            {
                DatabaseAttribute? attribute = type.GetCustomAttribute<DatabaseAttribute>();

                fileConfiguredDict.TryGetValue(type.FullName!, out DatabaseModelSetting? fileConfigured);

                DatabaseModelSetting modelSchema = new DatabaseModelSetting
                {
                    ModelTypeFullName = type.FullName!
                };

                if (attribute != null)
                {
                    modelSchema.DatabaseName = attribute.DatabaseName.IsNullOrEmpty() ? databaseEngine.FirstDefaultDatabaseName : attribute.DatabaseName!;

                    if (attribute.TableName.IsNullOrEmpty())
                    {
                        modelSchema.TableName = "tb_";

                        if (type.Name.EndsWith(attribute.SuffixToRemove, GlobalSettings.Comparison))
                        {
#if NETSTANDARD2_1 || NET5_0_OR_GREATER
                            modelSchema.TableName += type.Name[..^attribute.SuffixToRemove.Length].ToLower(GlobalSettings.Culture);
#elif NETSTANDARD2_0
                            modelSchema.TableName += type.Name.Substring(0, type.Name.Length - attribute.SuffixToRemove.Length).ToLower(GlobalSettings.Culture);
#endif
                        }
                        else
                        {
                            modelSchema.TableName += type.Name.ToLower(GlobalSettings.Culture);
                        }
                    }
                    else
                    {
                        modelSchema.TableName = attribute.TableName!;
                    }

                    modelSchema.Description = attribute.Description;
                    modelSchema.ReadOnly = attribute.ReadOnly;
                }

                //文件配置可以覆盖代码中的配置
                if (fileConfigured != null)
                {
                    if (!string.IsNullOrEmpty(fileConfigured.DatabaseName))
                    {
                        modelSchema.DatabaseName = fileConfigured.DatabaseName;
                    }

                    if (!string.IsNullOrEmpty(fileConfigured.TableName))
                    {
                        modelSchema.TableName = fileConfigured.TableName;
                    }

                    if (!string.IsNullOrEmpty(fileConfigured.Description))
                    {
                        modelSchema.Description = fileConfigured.Description;
                    }

                    modelSchema.ReadOnly = fileConfigured.ReadOnly;
                }

                //做最后的检查，有可能两者都没有定义
                if (modelSchema.DatabaseName.IsNullOrEmpty())
                {
                    modelSchema.DatabaseName = databaseEngine.FirstDefaultDatabaseName;
                }

                if (modelSchema.TableName.IsNullOrEmpty())
                {
                    modelSchema.TableName = "tb_" + type.Name.ToLower(GlobalSettings.Culture);
                }

                resusltModelSchemaDict.Add(type.FullName!, modelSchema);
            }

            return resusltModelSchemaDict;
        }

        public DatabaseModelDef? GetDef<T>() where T : DatabaseModel
        {
            return GetDef(typeof(T));
        }

        public DatabaseModelDef? GetDef(Type? modelType)
        {
            if (modelType == null)
            {
                return null;
            }

            if (_defDict.TryGetValue(modelType, out DatabaseModelDef? modelDef))
            {
                return modelDef;
            }

            return null;
        }

        private static DatabaseModelDef CreateModelDef(Type modelType, EngineType engineType, IDictionary<string, DatabaseModelSetting> modelSettingDict)
        {
            //GlobalSettings.Logger.LogInformation($"{modelType} : {modelType.GetHashCode()}");

            if (!modelSettingDict!.TryGetValue(modelType.FullName!, out DatabaseModelSetting? dbSchema))
            {
                throw DatabaseExceptions.ModelError(type: modelType.FullName, "", cause: "不是Model，或者没有DatabaseModelAttribute.");
            }

            DatabaseModelDef modelDef = new DatabaseModelDef
            {
                ModelType = modelType,
                ModelFullName = modelType.FullName!,
                DatabaseName = dbSchema.DatabaseName,
                TableName = dbSchema.TableName,
                
                IsServerDatabaseModel = typeof(ServerDatabaseModel).IsAssignableFrom(modelType),
                IsIdAutoIncrement = typeof(AutoIncrementIdDatabaseModel).IsAssignableFrom(modelType) || typeof(ClientAutoIncrementIdDatabaseModel).IsAssignableFrom(modelType),
                IsIdGuid = typeof(GuidDatabaseModel).IsAssignableFrom(modelType) || typeof(ClientGuidDatabaseModel).IsAssignableFrom(modelType),
                IsIdLong = typeof(LongIdDatabaseModel).IsAssignableFrom( modelType) || typeof(ClientLongIdDatabaseModel).IsAssignableFrom(modelType)
            };

            modelDef.DbTableReservedName = SqlHelper.GetReserved(modelDef.TableName!, engineType);
            modelDef.DatabaseWriteable = !dbSchema.ReadOnly;

            //确保Id排在第一位，在ModelMapper中，判断reader.GetValue(0)为DBNull,则为Null
            var orderedProperties = modelType.GetProperties().OrderBy(p => p, new PropertyOrderComparer());

            foreach (PropertyInfo info in orderedProperties)
            {
                DatabaseModelPropertyAttribute? modelPropertyAttribute = info.GetCustomAttribute<DatabaseModelPropertyAttribute>(true);

                if (modelPropertyAttribute == null)
                {
                    IgnoreModelPropertyAttribute? ignoreAttribute = info.GetCustomAttribute<IgnoreModelPropertyAttribute>(true);

                    if (ignoreAttribute != null)
                    {
                        continue;
                    }
                    else
                    {
                        modelPropertyAttribute = new DatabaseModelPropertyAttribute();
                    }

                    if (info.Name == nameof(ServerDatabaseModel.LastUser))
                    {
                        modelPropertyAttribute.MaxLength = DefaultLengthConventions.MAX_LAST_USER_LENGTH;
                    }
                }

                DatabaseModelPropertyDef propertyDef = CreatePropertyDef(modelDef, info, modelPropertyAttribute, engineType);

                modelDef.FieldCount++;

                if (propertyDef.IsUnique)
                {
                    modelDef.UniqueFieldCount++;
                }

                modelDef.PropertyDefs.Add(propertyDef);
                modelDef.PropertyDict.Add(propertyDef.Name, propertyDef);
            }

            return modelDef;
        }

        private static DatabaseModelPropertyDef CreatePropertyDef(DatabaseModelDef modelDef, PropertyInfo propertyInfo, DatabaseModelPropertyAttribute propertyAttribute, EngineType engineType)
        {
            DatabaseModelPropertyDef propertyDef = new DatabaseModelPropertyDef
            {
                ModelDef = modelDef,
                Name = propertyInfo.Name,
                Type = propertyInfo.PropertyType
            };
            propertyDef.NullableUnderlyingType = Nullable.GetUnderlyingType(propertyDef.Type);

            propertyDef.SetMethod = propertyInfo.GetSetterMethod(modelDef.ModelType)
                ?? throw DatabaseExceptions.ModelError(type: modelDef.ModelFullName, propertyName: propertyInfo.Name, cause: "实体属性缺少Set方法. ");

            propertyDef.GetMethod = propertyInfo.GetGetterMethod(modelDef.ModelType)
                ?? throw DatabaseExceptions.ModelError(type: modelDef.ModelFullName, propertyName: propertyInfo.Name, cause: "实体属性缺少Get方法. ");

            propertyDef.IsIndexNeeded = propertyAttribute.NeedIndex;
            propertyDef.IsNullable = !propertyAttribute.NotNull;
            propertyDef.IsUnique = propertyAttribute.Unique;
            propertyDef.DbMaxLength = propertyAttribute.MaxLength > 0 ? (int?)propertyAttribute.MaxLength : null;
            propertyDef.IsLengthFixed = propertyAttribute.FixedLength;

            propertyDef.DbReservedName = SqlHelper.GetReserved(propertyDef.Name, engineType);
            propertyDef.DbParameterizedName = SqlHelper.GetParameterized(propertyDef.Name);

            if (propertyAttribute.Converter != null)
            {
                propertyDef.TypeConverter = (ITypeConverter)Activator.CreateInstance(propertyAttribute.Converter)!;
            }

            //判断是否是主键
            PrimaryKeyAttribute? primaryAttribute = propertyInfo.GetCustomAttribute<PrimaryKeyAttribute>(false);

            if (primaryAttribute != null)
            {
                modelDef.PrimaryKeyPropertyDef = propertyDef;
                propertyDef.IsPrimaryKey = true;
                propertyDef.IsAutoIncrementPrimaryKey = primaryAttribute is AutoIncrementPrimaryKeyAttribute;
                propertyDef.IsNullable = false;
                propertyDef.IsForeignKey = false;
                propertyDef.IsUnique = true;
            }
            else
            {
                //判断是否外键
                ForeignKeyAttribute? atts2 = propertyInfo.GetCustomAttribute<ForeignKeyAttribute>(false);

                if (atts2 != null)
                {
                    propertyDef.IsAutoIncrementPrimaryKey = false;
                    propertyDef.IsForeignKey = true;
                    propertyDef.IsNullable = true;
                    propertyDef.IsUnique = atts2.IsUnique;
                }
            }

            return propertyDef;
        }

        public IEnumerable<DatabaseModelDef> GetAllDefsByDatabase(string databaseName)
        {
            return _defDict.Values.Where(def => databaseName.Equals(def.DatabaseName, GlobalSettings.ComparisonIgnoreCase));
        }

        public ITypeConverter? GetPropertyTypeConverter(Type modelType, string propertyName)
        {
            return GetDef(modelType)?.GetPropertyDef(propertyName)!.TypeConverter;
        }
    }
}