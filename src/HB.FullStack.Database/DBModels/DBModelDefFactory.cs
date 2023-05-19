using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database.DbModels
{
    internal class DbModelDefFactory : IDbModelDefFactory, IModelDefProvider
    {
        private class DbTableSchemaEx
        {
            public DbSchema DbSchema { get; set; } = null!;

            public DbTableSchema TableSchema { get; set; } = null!;
        }

        private readonly IDbConfigManager _configManager;

        /// <summary>
        /// 这里不用ConcurrentDictionary。是因为在初始化时，就已经ConstructModelDefs，后续只有read，没有write
        /// </summary>
        private readonly IDictionary<Type, DbModelDef> _dbModelDefs = new Dictionary<Type, DbModelDef>();

        public DbModelDefFactory(IDbConfigManager configManager)
        {
            _configManager = configManager;

            static bool typeCondition(Type t) => t.IsSubclassOf(typeof(BaseDbModel)) && !t.IsAbstract;

            IEnumerable<Type> allModelTypes = _configManager.DbModelAssemblies.IsNullOrEmpty()
                ? ReflectionUtil.GetAllTypeByCondition(typeCondition)
                : ReflectionUtil.GetAllTypeByCondition(_configManager.DbModelAssemblies, typeCondition);

            Dictionary<Type, DbTableSchemaEx> typeDbTableSchemaDict = ConstructDbTableSchema(allModelTypes);

            ConstructDbModelDefs(allModelTypes, typeDbTableSchemaDict);

            CheckDuplicateTableNames();

            Dictionary<Type, DbTableSchemaEx> ConstructDbTableSchema(IEnumerable<Type> allModelTypes)
            {
                Dictionary<Type, DbTableSchemaEx> resultTypeDbTableSchemaDict = new Dictionary<Type, DbTableSchemaEx>();

                IDictionary<string, DbTableSchemaEx> typeDbTableSchemaFromOptions = new Dictionary<string, DbTableSchemaEx>();

                foreach (DbSchema schema in _configManager.AllDbSchemas)
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
                        ReadOnly = false,
                        ConflictCheckMethods = ConflictCheckMethods.Timestamp | ConflictCheckMethods.OldNewValueCompare
                    };

                    string? resultDbSchemaName = null;

                    DbModelAttribute? modelAttribute = type.GetCustomAttribute<DbModelAttribute>(true);

                    //来自Attribute
                    if (modelAttribute != null)
                    {
                        resultDbSchemaName = modelAttribute.DbSchemaName;
                        resultTableSchema.TableName = modelAttribute.TableName ?? resultTableSchema.TableName;
                        resultTableSchema.ReadOnly = modelAttribute.ReadOnly ?? resultTableSchema.ReadOnly;
                        resultTableSchema.ConflictCheckMethods = modelAttribute.ConflictCheckMethods ?? resultTableSchema.ConflictCheckMethods;
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
                        resultDbSchemaName = _configManager.DefaultDbSchema.Name;
                    }

                    DbSchema resultDbSchema = _configManager.GetDbSchema(resultDbSchemaName);

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
                    string key = $"{modelDef.DbSchema.Name} + {modelDef.TableName}";

                    if (!hashSet.Add(key))
                    {
                        throw DbExceptions.SameTableNameInSameDbSchema(modelDef.DbSchema.Name, modelDef.TableName);
                    }
                }
            }
            _configManager = configManager;
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

        private DbModelDef CreateModelDef(Type modelType, DbTableSchema tableSchema, DbSchema dbSchema)
        {
            DbModelDef modelDef = new DbModelDef
            {
                Kind = ModelKind.Db,
                FullName = modelType.FullName!,
                ModelType = modelType,
                IsPropertyTrackable = modelType.IsAssignableTo(typeof(IPropertyTrackableObject)),

                DbSchema = dbSchema,
                EngineType = dbSchema.EngineType,
                Engine = dbSchema.Engine,
                TableName = tableSchema.TableName,
                IsTimestamp = typeof(ITimestamp).IsAssignableFrom(modelType),
                IsWriteable = !(tableSchema.ReadOnly!.Value),
            };

            //AllowedConflictCheckMethods
            modelDef.AllowedConflictCheckMethods = tableSchema.ConflictCheckMethods!.Value;

            if (!modelDef.IsTimestamp && tableSchema.ConflictCheckMethods!.Value.HasFlag(ConflictCheckMethods.Timestamp))
            {
                modelDef.AllowedConflictCheckMethods ^= ConflictCheckMethods.Timestamp;
            }

            modelDef.BestConflictCheckMethodWhenUpdate = GetBestConflictCheckMethodWhenUpdate(modelDef);
            modelDef.BestConflictCheckMethodWhenDelete = GetBestConflictCheckMethodWhenDelete(modelDef);

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

        private static ConflictCheckMethods GetBestConflictCheckMethodWhenUpdate(DbModelDef modelDef)
        {
            if (modelDef.IsTimestamp && modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Timestamp))
            {
                return ConflictCheckMethods.Timestamp;
            }

            if (modelDef.IsPropertyTrackable && modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.OldNewValueCompare))
            {
                return ConflictCheckMethods.OldNewValueCompare;
            }

            if (modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Ignore))
            {
                return ConflictCheckMethods.Ignore;
            }

            throw DbExceptions.ConflictCheckError($"{modelDef.FullName} can not get best conflict check method for update. allowed methods:{modelDef.AllowedConflictCheckMethods}");
        }

        private static ConflictCheckMethods GetBestConflictCheckMethodWhenDelete(DbModelDef modelDef)
        {
            if (modelDef.IsTimestamp && modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Timestamp))
            {
                return ConflictCheckMethods.Timestamp;
            }

            if (modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.OldNewValueCompare))
            {
                return ConflictCheckMethods.OldNewValueCompare;
            }

            if (modelDef.AllowedConflictCheckMethods.HasFlag(ConflictCheckMethods.Ignore))
            {
                return ConflictCheckMethods.Ignore;
            }

            throw DbExceptions.ConflictCheckError($"{modelDef.FullName} can not get best conflict check method for delete. allowed methods:{modelDef.AllowedConflictCheckMethods}");
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
            var primaryAttributes = propertyInfo.GetCustomAttributes<DbPrimaryKeyAttribute>(true);

            if (primaryAttributes.IsNotNullOrEmpty())
            {
                if (propertyInfo.Name != nameof(DbModel2<long>.Id))
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
            return _dbModelDefs.Values.Where(def => dbSchemaName.Equals(def.DbSchema.Name, Globals.ComparisonIgnoreCase));
        }

        public IDbPropertyConverter? GetPropertyTypeConverter(Type modelType, string propertyName)
        {
            return GetDef(modelType)?.GetDbPropertyDef(propertyName)!.TypeConverter;
        }

        ModelKind IModelDefProvider.ModelKind => ModelKind.Db;

        ModelDef? IModelDefProvider.GetModelDef(Type type) => GetDef(type);
    }
}