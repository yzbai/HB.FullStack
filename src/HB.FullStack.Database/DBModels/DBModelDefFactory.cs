using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.PropertyTrackable;
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

            //从Options中获取
            ConstructDbTableSchemaFromOptions(allModelTypes, out Dictionary<Type, DbTableSchemaEx> typeSchemaDictFromOptions);
            
            //从程序中获取
            ConstructDbModelDefFromProgramming(allModelTypes, typeSchemaDictFromOptions);

            CheckDbModelDefs();

            void ConstructDbTableSchemaFromOptions(IEnumerable<Type> allModelTypes, out Dictionary<Type, DbTableSchemaEx> typeSchemaDictFromOptions)
            {
                typeSchemaDictFromOptions = new Dictionary<Type, DbTableSchemaEx>();

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

                    DbModelAttribute? tableAttribute = type.GetCustomAttribute<DbModelAttribute>(true);

                    //来自Attribute
                    if (tableAttribute != null)
                    {
                        resultDbSchemaName = tableAttribute.DbSchemaName;
                        resultTableSchema.TableName = tableAttribute.TableName ?? resultTableSchema.TableName;
                        resultTableSchema.ReadOnly = tableAttribute.ReadOnly ?? resultTableSchema.ReadOnly;
                        resultTableSchema.ConflictCheckMethod = tableAttribute.ConflictCheckMethod;
                    }

                    //来自Options, 覆盖Attribute
                    if (typeTableFromOptionsDict.TryGetValue(type.FullName!, out DbTableSchemaEx? optionTableSchemaEx))
                    {
                        resultDbSchemaName = optionTableSchemaEx.DbSchema.Name ?? resultDbSchemaName;
                        resultTableSchema.TableName = optionTableSchemaEx.TableSchema.TableName ?? resultTableSchema.TableName;
                        resultTableSchema.ReadOnly = optionTableSchemaEx.TableSchema.ReadOnly ?? resultTableSchema.ReadOnly;
                        resultTableSchema.Fields = optionTableSchemaEx.TableSchema.Fields ?? resultTableSchema.Fields;
                        resultTableSchema.ConflictCheckMethod = optionTableSchemaEx.TableSchema.ConflictCheckMethod ?? resultTableSchema.ConflictCheckMethod;
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

                    typeSchemaDictFromOptions.Add(type, new DbTableSchemaEx { DbSchema = resultDbSchema, TableSchema = resultTableSchema });
                }

                //return resultTypeTableDict;
            }

            void ConstructDbModelDefFromProgramming(IEnumerable<Type> types, IDictionary<Type, DbTableSchemaEx> typeTableSchemaDictFromOptions)
            {
                foreach (Type type in types)
                {
                    if (!typeTableSchemaDictFromOptions!.TryGetValue(type, out DbTableSchemaEx? dbTableSchemaExFromOptions))
                    {
                        throw DbExceptions.ModelError(type: type.FullName, "", cause: "不是Model，或者没有DatabaseModelAttribute.");
                    }

                    _dbModelDefs[type] = CreateModelDef(type, dbTableSchemaExFromOptions.TableSchema, dbTableSchemaExFromOptions.DbSchema);
                }
            }

            static DbModelDef CreateModelDef(Type modelType, DbTableSchema tableSchemaFromOptons, DbSchema dbSchemaFromOptions)
            {
                DbModelDef modelDef = new DbModelDef
                {
                    Kind = ModelKind.Db,
                    ModelFullName = modelType.FullName!,
                    ModelType = modelType,
                    IsPropertyTrackable = modelType.IsAssignableTo(typeof(IPropertyTrackableObject)),

                    DbSchemaName = dbSchemaFromOptions.Name,
                    EngineType = dbSchemaFromOptions.EngineType,

                    TableName = tableSchemaFromOptons.TableName,
                    IdType = GetIdType(modelType),
                    HasTimestamp = typeof(TimestampDbModel).IsAssignableFrom(modelType),

                    IsWriteable = !(tableSchemaFromOptons.ReadOnly!.Value),

                    ConflictCheckMethod = tableSchemaFromOptons.ConflictCheckMethod ?? DbConflictCheckMethod.Both
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
                            fieldAttribute.MaxLength = dbSchemaFromOptions.MaxLastUserFieldLength;
                        }
                    }

                    DbFieldSchema? fieldSchemaFromOptions = tableSchemaFromOptons.Fields.FirstOrDefault(f => f.FieldName == propertyInfo.Name);
                    DbModelPropertyDef propertyDef = CreatePropertyDef(modelDef, propertyInfo, fieldAttribute, fieldSchemaFromOptions, dbSchemaFromOptions);

                    modelDef.FieldCount++;

                    if (propertyDef.IsUnique)
                    {
                        modelDef.UniqueFieldCount++;
                    }

                    modelDef.PropertyDefs.Add(propertyDef);
                    modelDef.PropertyDict.Add(propertyDef.Name, propertyDef);
                }

                return modelDef;

                static DbModelIdType GetIdType(Type modelType)
                {
                    if(typeof(IAutoIncrementId).IsAssignableFrom(modelType))
                    {
                        return DbModelIdType.AutoIncrementLongId;
                    }
                    if(typeof(IGuidId).IsAssignableFrom(modelType))
                    {
                        return DbModelIdType.GuidId;
                    }
                    if(typeof(ILongId).IsAssignableFrom(modelType))
                    {
                        return DbModelIdType.LongId;
                    }

                    throw new ErrorCodeException(ErrorCodes.ModelDefError, $"{modelType.FullName} has unkown DbModelIdType.");
                }
            }

            static DbModelPropertyDef CreatePropertyDef(DbModelDef modelDef, PropertyInfo propertyInfo, DbFieldAttribute fieldAttribute, DbFieldSchema? fieldSchemaFromOptions, DbSchema dbSchema)
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

                propertyDef.IsIndexNeeded = fieldSchemaFromOptions?.NeedIndex ?? fieldAttribute.NeedIndex;
                propertyDef.IsNullable = !(fieldSchemaFromOptions?.NotNull ?? fieldAttribute.NotNull);
                propertyDef.IsUnique = fieldSchemaFromOptions?.Unique ?? fieldAttribute.Unique;
                propertyDef.DbMaxLength = fieldSchemaFromOptions?.MaxLength ?? (fieldAttribute.MaxLength > 0 ? (int?)fieldAttribute.MaxLength : null);
                propertyDef.IsLengthFixed = fieldSchemaFromOptions?.FixedLength ?? fieldAttribute.FixedLength;

                propertyDef.DbReservedName = SqlHelper.GetReserved(propertyDef.Name, dbSchema.EngineType);
                propertyDef.DbParameterizedName = SqlHelper.GetParameterized(propertyDef.Name);

                if (fieldAttribute.Converter != null)
                {
                    propertyDef.TypeConverter = (IDbPropertyConverter)Activator.CreateInstance(fieldAttribute.Converter)!;
                }

                //判断是否是主键
                DbPrimaryKeyAttribute? primaryAttribute = propertyInfo.GetCustomAttribute<DbPrimaryKeyAttribute>(true);

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
                    DbForeignKeyAttribute? atts2 = propertyInfo.GetCustomAttribute<DbForeignKeyAttribute>(true);

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