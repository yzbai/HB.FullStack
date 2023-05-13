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

            static bool typeCondition(Type t) => t.IsSubclassOf(typeof(BaseDbModel)) && !t.IsAbstract;

            IEnumerable<Type> allModelTypes = _options.DbModelAssemblies.IsNullOrEmpty()
                ? ReflectionUtil.GetAllTypeByCondition(typeCondition)
                : ReflectionUtil.GetAllTypeByCondition(_options.DbModelAssemblies, typeCondition);

            Dictionary<Type, DbTableSchemaEx> typeDbTableSchemaDict = ConstructDbTableSchema(allModelTypes);

            ConstructDbModelDefs(allModelTypes, typeDbTableSchemaDict);

            CheckDuplicateTableNames();

            Dictionary<Type, DbTableSchemaEx> ConstructDbTableSchema(IEnumerable<Type> allModelTypes)
            {
                Dictionary<Type, DbTableSchemaEx> resultTypeDbTableSchemaDict = new Dictionary<Type, DbTableSchemaEx>();

                IDictionary<string, DbTableSchemaEx> typeDbTableSchemaFromOptions = new Dictionary<string, DbTableSchemaEx>();

                foreach (DbSchema schema in _options.DbSchemas)
                {
                    foreach (DbTableSchema tableSchema in schema.Tables)
                    {
                        DbTableSchemaEx dbTableSchemaEx = new DbTableSchemaEx { DbSchema = schema, TableSchema = tableSchema };

                        if (!typeDbTableSchemaFromOptions.TryAdd(tableSchema.DbModelFullName, dbTableSchemaEx))
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
                        resultTableSchema.ConflictCheckMethods = tableAttribute.ConflictCheckMethods;
                    }

                    //来自Options, 覆盖Attribute
                    if (typeDbTableSchemaFromOptions.TryGetValue(type.FullName!, out DbTableSchemaEx? optionTableSchemaEx))
                    {
                        resultDbSchemaName = optionTableSchemaEx.DbSchema.Name ?? resultDbSchemaName;
                        resultTableSchema.TableName = optionTableSchemaEx.TableSchema.TableName ?? resultTableSchema.TableName;
                        resultTableSchema.ReadOnly = optionTableSchemaEx.TableSchema.ReadOnly ?? resultTableSchema.ReadOnly;
                        resultTableSchema.Fields = optionTableSchemaEx.TableSchema.Fields ?? resultTableSchema.Fields;
                        resultTableSchema.ConflictCheckMethods = optionTableSchemaEx.TableSchema.ConflictCheckMethods ?? resultTableSchema.ConflictCheckMethods;
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

                    resultTypeDbTableSchemaDict.Add(type, new DbTableSchemaEx { DbSchema = resultDbSchema, TableSchema = resultTableSchema });
                }

                return resultTypeDbTableSchemaDict;
            }

            void CheckDuplicateTableNames()
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

        private void ConstructDbModelDefs(IEnumerable<Type> types, IDictionary<Type, DbTableSchemaEx> typeDbTableSchemaDict)
        {
            foreach (Type type in types)
            {
                if (!typeDbTableSchemaDict!.TryGetValue(type, out DbTableSchemaEx? dbTableSchemaExFromOptions))
                {
                    throw DbExceptions.ModelError(type: type.FullName, "", cause: "不是Model，或者没有DatabaseModelAttribute.");
                }

                _dbModelDefs[type] = CreateModelDef(type, dbTableSchemaExFromOptions.TableSchema, dbTableSchemaExFromOptions.DbSchema);
            }
        }

        private static DbModelDef CreateModelDef(Type modelType, DbTableSchema tableSchema, DbSchema dbSchema)
        {
            DbModelDef modelDef = new DbModelDef
            {
                Kind = ModelKind.Db,
                FullName = modelType.FullName!,
                ModelType = modelType,
                IsPropertyTrackable = modelType.IsAssignableTo(typeof(IPropertyTrackableObject)),

                DbSchemaName = dbSchema.Name,
                EngineType = dbSchema.EngineType,

                TableName = tableSchema.TableName,
                IsTimestamp = typeof(ITimestamp).IsAssignableFrom(modelType),
                IsWriteable = !(tableSchema.ReadOnly!.Value),
            };

            //AllowedConflictCheckMethods
            modelDef.AllowedConflictCheckMethods = tableSchema.ConflictCheckMethods!.Value;

            if (!modelDef.IsTimestamp && tableSchema.ConflictCheckMethods!.Value.HasFlag(DbConflictCheckMethods.Timestamp))
            {
                modelDef.AllowedConflictCheckMethods ^= DbConflictCheckMethods.Timestamp;
            }

            modelDef.BestConflictCheckMethodWhenUpdateEntire = GetBestConflictCheckMethodWhenUpdateEntire(modelDef);

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

                    if (propertyInfo.Name == nameof(DbModel2<long>.LastUser))
                    {
                        fieldAttribute.MaxLength = dbSchema.MaxLastUserFieldLength;
                    }
                }

                DbFieldSchema? fieldSchemaFromOptions = tableSchema.Fields.FirstOrDefault(f => f.FieldName == propertyInfo.Name);
                DbModelPropertyDef propertyDef = CreatePropertyDef(modelDef, propertyInfo, fieldAttribute, fieldSchemaFromOptions, dbSchema);

                modelDef.FieldCount++;

                if (propertyDef.IsUnique)
                {
                    modelDef.UniqueFieldCount++;
                }

                modelDef.PropertyDefs.Add(propertyDef);
                modelDef.PropertyDict.Add(propertyDef.Name, propertyDef);
            }

            //TimestampPropertyDef
            if (modelDef.IsTimestamp)
            {
                modelDef.TimestampPropertyDef = modelDef.GetDbPropertyDef(nameof(ITimestamp.Timestamp)).ThrowIfNull($"{modelDef.FullName} should has a Timestamp Property!");
            }

            //IdType
            DbModelPropertyDef primaryKeyPropertyDef = modelDef.PrimaryKeyPropertyDef;

            if (primaryKeyPropertyDef != null)
            {
                if (primaryKeyPropertyDef.IsAutoIncrementPrimaryKey)
                {
                    modelDef.IdType = DbModelIdType.AutoIncrementLongId;
                }
                else if (primaryKeyPropertyDef.Type == typeof(long))
                {
                    modelDef.IdType = DbModelIdType.LongId;
                }
                else if (primaryKeyPropertyDef.Type == typeof(Guid))
                {
                    modelDef.IdType = DbModelIdType.GuidId;
                }
            }

            return modelDef;
        }

        private static DbConflictCheckMethods GetBestConflictCheckMethodWhenUpdateEntire(DbModelDef modelDef)
        {
            if (modelDef.IsTimestamp && modelDef.AllowedConflictCheckMethods.HasFlag(DbConflictCheckMethods.Timestamp))
            {
                return DbConflictCheckMethods.Timestamp;
            }

            if (modelDef.IsPropertyTrackable && modelDef.AllowedConflictCheckMethods.HasFlag(DbConflictCheckMethods.OldNewValueCompare))
            {
                return DbConflictCheckMethods.OldNewValueCompare;
            }

            if (modelDef.AllowedConflictCheckMethods.HasFlag(DbConflictCheckMethods.Ignore))
            {
                return DbConflictCheckMethods.Ignore;
            }

            throw DbExceptions.ConflictCheckError($"{modelDef.FullName} can not get proper conflict check method. allowed methods:{modelDef.AllowedConflictCheckMethods}");
        }

        private static DbModelPropertyDef CreatePropertyDef(DbModelDef modelDef, PropertyInfo propertyInfo, DbFieldAttribute fieldAttribute, DbFieldSchema? fieldSchemaFromOptions, DbSchema dbSchema)
        {
            DbModelPropertyDef propertyDef = new DbModelPropertyDef
            {
                ModelDef = modelDef,
                Name = propertyInfo.Name,
                Type = propertyInfo.PropertyType
            };
            propertyDef.NullableUnderlyingType = Nullable.GetUnderlyingType(propertyDef.Type);

            propertyDef.SetMethod = propertyInfo.GetSetterMethod(modelDef.ModelType)
                ?? throw DbExceptions.ModelError(type: modelDef.FullName, propertyName: propertyInfo.Name, cause: "实体属性缺少Set方法. ");

            propertyDef.GetMethod = propertyInfo.GetGetterMethod(modelDef.ModelType)
                ?? throw DbExceptions.ModelError(type: modelDef.FullName, propertyName: propertyInfo.Name, cause: "实体属性缺少Get方法. ");

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
                if(propertyInfo.Name != nameof(DbModel2<long>.Id))
                {
                    throw DbExceptions.ModelError($"the name of PrimaryKey of {modelDef.FullName} should always be 'Id', but '{propertyInfo.Name}' ");
                }

                modelDef.PrimaryKeyPropertyDef = propertyDef;
                propertyDef.IsPrimaryKey = true;
                propertyDef.IsAutoIncrementPrimaryKey = propertyInfo.GetCustomAttribute<DbAutoIncrementPrimaryKeyAttribute>(true) != null;
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

            if (propertyInfo.Name == nameof(BaseDbModel.Deleted))
            {
                modelDef.DeletedPropertyDef = propertyDef;
            }
            else if (propertyInfo.Name == nameof(BaseDbModel.LastUser))
            {
                modelDef.LastUserPropertyDef = propertyDef;
            }

            return propertyDef;
        }

        public DbModelDef? GetDef<T>() where T : BaseDbModel
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