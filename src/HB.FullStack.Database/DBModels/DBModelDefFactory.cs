using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.SQL;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Database.DbModels
{
    internal class DbModelDefFactory : IDbModelDefFactory, IModelDefProvider
    {
        class DbTableSchemaEx
        {
            public DbSchema DbSchema { get; set; } = null!;

            public DbTableSchema TableSchema { get; set; } = null!;
        }

        private readonly DbOptions _options;

        /// <summary>
        /// 这里不用ConcurrentDictionary。是因为在初始化时，就已经ConstructModelDefs，后续只有read，没有write
        /// </summary>
        private readonly IDictionary<Type, DbModelDef> _dbModelDefs = new Dictionary<Type, DbModelDef>();

        public DbModelDefFactory(IOptions<DbOptions> options)
        {
            _options = options.Value;

            static bool typeCondition(Type t) => t.IsSubclassOf(typeof(DbModel)) && !t.IsAbstract;

            IEnumerable<Type> allModelTypes = _options.DbModelAssemblies.IsNullOrEmpty()
                ? ReflectionUtil.GetAllTypeByCondition(typeCondition)
                : ReflectionUtil.GetAllTypeByCondition(_options.DbModelAssemblies, typeCondition);

            IDictionary<Type, DbTableSchemaEx> typeSchemaDict = FlatDbTableSchemas(allModelTypes);

            ConstructDbModelDefs(allModelTypes, typeSchemaDict);

            CheckDbModelDefs();

            IDictionary<Type, DbTableSchemaEx> FlatDbTableSchemas(IEnumerable<Type> allModelTypes)
            {
                IDictionary<Type, DbTableSchemaEx> resultTypeTableDict = new Dictionary<Type, DbTableSchemaEx>();
                IDictionary<string, DbTableSchemaEx> typeTableFromOptionsDict = new Dictionary<string, DbTableSchemaEx>();

                foreach (DbSchema schema in _options.DbSchemas)
                {
                    foreach (DbTableSchema tableSchema in schema.Tables)
                    {
                        DbTableSchemaEx dbTableSchemaEx = new DbTableSchemaEx { DbSchema = schema, TableSchema = tableSchema };

                        if (!typeTableFromOptionsDict.TryAdd(tableSchema.DbModelFullName, dbTableSchemaEx))
                        {
                            throw DbExceptions.DbSchemaError(schema.Name, $"Same DbModel FullName :{tableSchema.DbModelFullName} Exists Already.");
                        }
                    }
                }

                foreach (Type type in allModelTypes)
                {
                    DbTableSchema resultTableSchema = new DbTableSchema
                    {
                        DbModelFullName = type.FullName!,
                        TableName = "tb_" + type.Name,
                        ReadOnly = false
                    };

                    string resultDbSchemaName = null!;

                    DbTableAttribute? tableAttribute = type.GetCustomAttribute<DbTableAttribute>();

                    //来自Attribute
                    if (tableAttribute != null)
                    {
                        resultDbSchemaName = tableAttribute.DbSchemaName;
                        resultTableSchema.TableName = tableAttribute.TableName ?? resultTableSchema.TableName;
                        resultTableSchema.ReadOnly = tableAttribute.ReadOnly ?? resultTableSchema.ReadOnly;
                    }

                    //来自Options, 覆盖Attribute
                    if (typeTableFromOptionsDict.TryGetValue(type.FullName!, out DbTableSchemaEx? optionTableSchemaEx))
                    {
                        resultDbSchemaName = optionTableSchemaEx.DbSchema.Name ?? resultDbSchemaName;
                        resultTableSchema.TableName = optionTableSchemaEx.TableSchema.TableName ?? resultTableSchema.TableName;
                        resultTableSchema.ReadOnly = optionTableSchemaEx.TableSchema.ReadOnly ?? resultTableSchema.ReadOnly;
                        resultTableSchema.Fields = optionTableSchemaEx.TableSchema.Fields ?? resultTableSchema.Fields;
                    }

                    //做最后的检查，有可能两者都没有定义, 默认使用第一个
                    if (resultDbSchemaName.IsNullOrEmpty())
                    {
                        resultDbSchemaName = _options.DbSchemas[0].Name;
                    }

                    DbSchema resultDbSchema = _options.DbSchemas.First(s => s.Name == resultDbSchemaName);

                    if (resultDbSchema.TableNameSuffixToRemove.IsNotNullOrEmpty())
                    {
                        resultTableSchema.TableName = resultTableSchema.TableName.RemoveSuffix(resultDbSchema.TableNameSuffixToRemove);
                    }

                    resultTypeTableDict.Add(type, new DbTableSchemaEx { DbSchema = resultDbSchema, TableSchema = resultTableSchema });
                }

                return resultTypeTableDict;
            }

            void ConstructDbModelDefs(IEnumerable<Type> types, IDictionary<Type, DbTableSchemaEx> typeTableSchemaDict)
            {
                foreach (Type type in types)
                {
                    if (!typeTableSchemaDict!.TryGetValue(type, out DbTableSchemaEx? dbTableSchemaEx))
                    {
                        throw DbExceptions.ModelError(type: type.FullName, "", cause: "不是Model，或者没有DatabaseModelAttribute.");
                    }

                    _dbModelDefs[type] = CreateModelDef(type, dbTableSchemaEx.TableSchema, dbTableSchemaEx.DbSchema);
                }
            }

            static DbModelDef CreateModelDef(Type modelType, DbTableSchema tableSchema, DbSchema dbSchema)
            {
                DbModelDef modelDef = new DbModelDef
                {
                    Kind = ModelKind.Db,
                    ModelFullName = modelType.FullName!,
                    ModelType = modelType,

                    DbSchemaName = dbSchema.Name,
                    EngineType = dbSchema.EngineType,

                    TableName = tableSchema.TableName,

                    IsTimestampDBModel = typeof(TimestampDbModel).IsAssignableFrom(modelType),
                    IsIdAutoIncrement = typeof(IAutoIncrementId).IsAssignableFrom(modelType),
                    IsIdGuid = typeof(IGuidId).IsAssignableFrom(modelType),
                    IsIdLong = typeof(ILongId).IsAssignableFrom(modelType),

                    DbWriteable = !(tableSchema.ReadOnly!.Value)
                };

                //确保Id排在第一位，在ModelMapper中，判断reader.GetValue(0)为DBNull,则为Null
                var orderedProperties = modelType.GetProperties().OrderBy(p => p, new PropertyOrderComparer());

                foreach (PropertyInfo propertyInfo in orderedProperties)
                {
                    DbFieldAttribute? fieldAttribute = propertyInfo.GetCustomAttribute<DbFieldAttribute>(true);

                    if (fieldAttribute == null)
                    {
                        DbIgnoreFieldPropertyAttribute? ignoreAttribute = propertyInfo.GetCustomAttribute<DbIgnoreFieldPropertyAttribute>(true);

                        if (ignoreAttribute != null)
                        {
                            continue;
                        }

                        fieldAttribute = new DbFieldAttribute();

                        if (propertyInfo.Name == nameof(TimestampDbModel.LastUser))
                        {
                            fieldAttribute.MaxLength = dbSchema.MaxLastUserFieldLength;
                        }
                    }

                    DbFieldSchema? fieldSchema = tableSchema.Fields.FirstOrDefault(f => f.FieldName == propertyInfo.Name);
                    DbModelPropertyDef propertyDef = CreatePropertyDef(modelDef, propertyInfo, fieldAttribute, fieldSchema, dbSchema);

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

            static DbModelPropertyDef CreatePropertyDef(DbModelDef modelDef, PropertyInfo propertyInfo, DbFieldAttribute fieldAttribute, DbFieldSchema? fieldSchema, DbSchema dbSchema)
            {
                DbModelPropertyDef propertyDef = new DbModelPropertyDef
                {
                    ModelDef = modelDef,
                    Name = propertyInfo.Name,
                    Type = propertyInfo.PropertyType
                };
                propertyDef.NullableUnderlyingType = Nullable.GetUnderlyingType(propertyDef.Type);

                propertyDef.SetMethod = propertyInfo.GetSetterMethod(modelDef.ModelType)
                    ?? throw DbExceptions.ModelError(type: modelDef.ModelFullName, propertyName: propertyInfo.Name, cause: "实体属性缺少Set方法. ");

                propertyDef.GetMethod = propertyInfo.GetGetterMethod(modelDef.ModelType)
                    ?? throw DbExceptions.ModelError(type: modelDef.ModelFullName, propertyName: propertyInfo.Name, cause: "实体属性缺少Get方法. ");

                propertyDef.IsIndexNeeded = fieldSchema?.NeedIndex ?? fieldAttribute.NeedIndex;
                propertyDef.IsNullable = !(fieldSchema?.NotNull ?? fieldAttribute.NotNull);
                propertyDef.IsUnique = fieldSchema?.Unique ?? fieldAttribute.Unique;
                propertyDef.DbMaxLength = fieldSchema?.MaxLength ?? (fieldAttribute.MaxLength > 0 ? (int?)fieldAttribute.MaxLength : null);
                propertyDef.IsLengthFixed = fieldSchema?.FixedLength ?? fieldAttribute.FixedLength;

                propertyDef.DbReservedName = SqlHelper.GetReserved(propertyDef.Name, dbSchema.EngineType);
                propertyDef.DbParameterizedName = SqlHelper.GetParameterized(propertyDef.Name);

                if (fieldAttribute.Converter != null)
                {
                    propertyDef.TypeConverter = (IDbPropertyConverter)Activator.CreateInstance(fieldAttribute.Converter)!;
                }

                //判断是否是主键
                DbPrimaryKeyAttribute? primaryAttribute = propertyInfo.GetCustomAttribute<DbPrimaryKeyAttribute>(false);

                if (primaryAttribute != null)
                {
                    modelDef.PrimaryKeyPropertyDef = propertyDef;
                    propertyDef.IsPrimaryKey = true;
                    propertyDef.IsAutoIncrementPrimaryKey = primaryAttribute is DbAutoIncrementPrimaryKeyAttribute;
                    propertyDef.IsNullable = false;
                    propertyDef.IsForeignKey = false;
                    propertyDef.IsUnique = true;
                }
                else
                {
                    //判断是否外键
                    DbForeignKeyAttribute? atts2 = propertyInfo.GetCustomAttribute<DbForeignKeyAttribute>(false);

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
                    string key = $"{modelDef.DbSchemaName} + {modelDef.TableName}";

                    if (!hashSet.Add(key))
                    {
                        throw DbExceptions.SameTableNameInSameDbSchema(modelDef.DbSchemaName, modelDef.TableName);
                    }
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

        public IEnumerable<DbModelDef> GetAllDefsByDbSchema(string dbSchemaName)
        {
            return _dbModelDefs.Values.Where(def => dbSchemaName.Equals(def.DbSchemaName, Globals.ComparisonIgnoreCase));
        }

        public IDbPropertyConverter? GetPropertyTypeConverter(Type modelType, string propertyName)
        {
            return GetDef(modelType)?.GetDbPropertyDef(propertyName)!.TypeConverter;
        }

        ModelKind IModelDefProvider.ModelKind => ModelKind.Db;

        ModelDef? IModelDefProvider.GetModelDef(Type type) => GetDef(type);
    }
}