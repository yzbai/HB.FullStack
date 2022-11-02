using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Database.DbModels
{
    internal class DbModelDefFactory : IDbModelDefFactory, IModelDefProvider
    {
        private readonly DatabaseOptions _options;

        /// <summary>
        /// 这里不用ConcurrentDictionary。是因为在初始化时，就已经ConstructModelDefs，后续只有read，没有write
        /// </summary>
        private readonly IDictionary<Type, DbModelDef> _dbModelDefs = new Dictionary<Type, DbModelDef>();

        public DbModelDefFactory(IOptions<DatabaseOptions> options)
        {
            _options = options.Value;

            static bool typeCondition(Type t) => t.IsSubclassOf(typeof(DbModel)) && !t.IsAbstract;

            IEnumerable<Type> allModelTypes = _options.DbModelAssemblies.IsNullOrEmpty()
                ? ReflectionUtil.GetAllTypeByCondition(typeCondition)
                : ReflectionUtil.GetAllTypeByCondition(_options.DbModelAssemblies, typeCondition);

            IDictionary<Type, (DbModelSetting, DbSetting)> dbModelSettings = RangeDbModelSettings(allModelTypes);

            ConstructDbModelDefs(allModelTypes, dbModelSettings);

            CheckDbModelDefs();

            IDictionary<Type, (DbModelSetting, DbSetting)> RangeDbModelSettings(IEnumerable<Type> allModelTypes)
            {
                IDictionary<Type, (DbModelSetting, DbSetting)> resusltSettings = new Dictionary<Type, (DbModelSetting, DbSetting)>();
                IDictionary<string, DbModelSetting> optionSettings = _options.DbModelSettings.ToDictionary(t => t.ModelFullName);

                foreach (Type type in allModelTypes)
                {
                    DbModelAttribute? dbModelAttribute = type.GetCustomAttribute<DbModelAttribute>();

                    DbModelSetting resultSetting = new DbModelSetting
                    {
                        ModelFullName = type.FullName!,
                        TableName = "tb_" + type.Name,
                        ReadOnly = false
                    };

                    //来自Attribute
                    if (dbModelAttribute != null)
                    {
                        //resultSetting.DbName = dbModelAttribute.DbName ?? resultSetting.DbName;
                        //resultSetting.DbKind = dbModelAttribute.DbKind ?? resultSetting.DbKind;
                        resultSetting.DbSchema = dbModelAttribute.DbSchema ?? resultSetting.DbSchema;
                        resultSetting.TableName = dbModelAttribute.TableName ?? resultSetting.TableName;
                        resultSetting.ReadOnly = dbModelAttribute.ReadOnly ?? resultSetting.ReadOnly;
                    }

                    //来自Options
                    if (optionSettings.TryGetValue(type.FullName!, out DbModelSetting? optionSetting))
                    {
                        //resultSetting.DbName = optionSetting.DbName ?? resultSetting.DbName;
                        //resultSetting.DbKind = optionSetting.DbKind ?? resultSetting.DbKind;
                        resultSetting.DbSchema = optionSetting.DbSchema ?? resultSetting.DbSchema;
                        resultSetting.TableName = optionSetting.TableName ?? resultSetting.TableName;
                        resultSetting.ReadOnly = optionSetting.ReadOnly ?? resultSetting.ReadOnly;
                    }

                    //做最后的检查，有可能两者都没有定义, 默认使用第一个
                    if (resultSetting.DbSchema.IsNullOrEmpty())
                    {
                        //resultSetting.DbName = _options.DbSettings[0].DbName;
                        //resultSetting.DbKind = _options.DbSettings[0].DbKind;
                        resultSetting.DbSchema = _options.DbSettings[0].DbSchema;
                    }

                    DbSetting dbSetting = _options.DbSettings.First(s => s.DbSchema == resultSetting.DbSchema);

                    if (dbSetting.TableNameSuffixToRemove.IsNotNullOrEmpty())
                    {
                        resultSetting.TableName = resultSetting.TableName.RemoveSuffix(dbSetting.TableNameSuffixToRemove);
                    }

                    resusltSettings.Add(type, (resultSetting, dbSetting));
                }

                return resusltSettings;
            }

            void ConstructDbModelDefs(IEnumerable<Type> types, IDictionary<Type, (DbModelSetting, DbSetting)> dbModelSettings)
            {
                foreach (Type type in types)
                {
                    if (!dbModelSettings!.TryGetValue(type, out var dbModelSetting))
                    {
                        throw DatabaseExceptions.ModelError(type: type.FullName, "", cause: "不是Model，或者没有DatabaseModelAttribute.");
                    }

                    _dbModelDefs[type] = CreateModelDef(type, dbModelSetting.Item1, dbModelSetting.Item2);
                }
            }

            static DbModelDef CreateModelDef(Type modelType, DbModelSetting dbModelSetting, DbSetting dbSetting)
            {
                DbModelDef modelDef = new DbModelDef
                {
                    Kind = ModelKind.Db,
                    ModelFullName = modelType.FullName!,
                    ModelType = modelType,

                    //DbName = dbModelSetting.DbName,
                    //DbKind = dbModelSetting.DbKind,
                    DbSchema = dbModelSetting.DbSchema,
                    EngineType = dbSetting.EngineType,

                    TableName = dbModelSetting.TableName,

                    IsTimestampDBModel = typeof(TimestampDbModel).IsAssignableFrom(modelType),
                    IsIdAutoIncrement = typeof(IAutoIncrementId).IsAssignableFrom(modelType),
                    IsIdGuid = typeof(IGuidId).IsAssignableFrom(modelType),
                    IsIdLong = typeof(ILongId).IsAssignableFrom(modelType),

                    DbWriteable = !(dbModelSetting.ReadOnly!.Value)
                };

                //确保Id排在第一位，在ModelMapper中，判断reader.GetValue(0)为DBNull,则为Null
                var orderedProperties = modelType.GetProperties().OrderBy(p => p, new PropertyOrderComparer());

                foreach (PropertyInfo info in orderedProperties)
                {
                    DbModelPropertyAttribute? modelPropertyAttribute = info.GetCustomAttribute<DbModelPropertyAttribute>(true);

                    if (modelPropertyAttribute == null)
                    {
                        IgnoreModelPropertyAttribute? ignoreAttribute = info.GetCustomAttribute<IgnoreModelPropertyAttribute>(true);

                        if (ignoreAttribute != null)
                        {
                            continue;
                        }
                        else
                        {
                            modelPropertyAttribute = new DbModelPropertyAttribute();
                        }

                        if (info.Name == nameof(TimestampDbModel.LastUser))
                        {
                            modelPropertyAttribute.MaxLength = DefaultLengthConventions.MAX_LAST_USER_LENGTH;
                        }
                    }

                    DbModelPropertyDef propertyDef = CreatePropertyDef(modelDef, info, modelPropertyAttribute, dbSetting);

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

            static DbModelPropertyDef CreatePropertyDef(DbModelDef modelDef, PropertyInfo propertyInfo, DbModelPropertyAttribute propertyAttribute, DbSetting dbSetting)
            {
                DbModelPropertyDef propertyDef = new DbModelPropertyDef
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

                propertyDef.DbReservedName = SqlHelper.GetReserved(propertyDef.Name, dbSetting.EngineType);
                propertyDef.DbParameterizedName = SqlHelper.GetParameterized(propertyDef.Name);

                if (propertyAttribute.Converter != null)
                {
                    propertyDef.TypeConverter = (IDbPropertyConverter)Activator.CreateInstance(propertyAttribute.Converter)!;
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

            void CheckDbModelDefs()
            {
                //Same table name under same dbschema
                HashSet<string> hashSet = new HashSet<string>();

                foreach (var modelDef in _dbModelDefs.Values)
                {
                    string key = $"{modelDef.DbSchema} + {modelDef.TableName}";

                    if (hashSet.Contains(key))
                    {
                        throw DatabaseExceptions.SameTableNameInSameDbSchema(modelDef.DbSchema, modelDef.TableName);
                    }

                    hashSet.Add(key);
                }
            }
        }

        public DbModelDef? GetDef<T>() where T : DbModel
        {
            return GetDef(typeof(T));
        }

        public DbModelDef? GetDef(Type? modelType)
        {
            if (modelType == null)
            {
                return null;
            }

            if (_dbModelDefs.TryGetValue(modelType, out DbModelDef? modelDef))
            {
                return modelDef;
            }

            return null;
        }

        public IEnumerable<DbModelDef> GetAllDefsByDbSchema(string dbSchema)
        {
            return _dbModelDefs.Values.Where(def => dbSchema.Equals(def.DbSchema, Globals.ComparisonIgnoreCase));
        }

        public IDbPropertyConverter? GetPropertyTypeConverter(Type modelType, string propertyName)
        {
            return GetDef(modelType)?.GetDbPropertyDef(propertyName)!.TypeConverter;
        }

        ModelKind IModelDefProvider.ModelKind => ModelKind.Db;

        ModelDef? IModelDefProvider.GetModelDef(Type type) => GetDef(type);
    }
}