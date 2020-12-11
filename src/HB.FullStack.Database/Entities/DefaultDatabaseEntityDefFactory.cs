using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HB.FullStack.Database.Entities
{
    /// <summary>
    /// 实体定义集合
    /// 多线程公用
    /// 单例
    /// </summary>
    internal class DefaultDatabaseEntityDefFactory : IDatabaseEntityDefFactory
    {
        public const int DEFAULT_VARCHAR_LENGTH = 200;

        private readonly object _lockObj = new object();
        private readonly DatabaseCommonSettings _databaseSettings;
        private readonly IDatabaseEngine _databaseEngine;
        private readonly ICustomTypeConverterFactory _typeConverterFactory;

        private readonly IDictionary<string, EntitySetting> _entitySchemaDict;
        private readonly IDictionary<Type, DatabaseEntityDef> _defDict = new Dictionary<Type, DatabaseEntityDef>();

        public DefaultDatabaseEntityDefFactory(IDatabaseEngine databaseEngine, ICustomTypeConverterFactory typeConverterFactory)
        {
            _databaseSettings = databaseEngine.DatabaseSettings;
            _databaseEngine = databaseEngine;
            _typeConverterFactory = typeConverterFactory;

            IEnumerable<Type> allEntityTypes;

            if (_databaseSettings.AssembliesIncludeEntity.IsNullOrEmpty())
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(entityTypeCondition);
            }
            else
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(_databaseSettings.AssembliesIncludeEntity, entityTypeCondition);
            }

            _entitySchemaDict = ConstructeSchemaDict(allEntityTypes);

            WarmUp(allEntityTypes);

            static bool entityTypeCondition(Type t)
            {
                return t.IsSubclassOf(typeof(Entity)) && !t.IsAbstract && t.GetCustomAttribute<DatabaseEntityAttribute>() != null;
            }
        }

        private void WarmUp(IEnumerable<Type> allEntityTypes)
        {
            allEntityTypes.ForEach(t => _defDict[t] = CreateEntityDef(t));
        }

        private IDictionary<string, EntitySetting> ConstructeSchemaDict(IEnumerable<Type> allEntityTypes)
        {
            IDictionary<string, EntitySetting> fileConfiguredDict = _databaseSettings.EntitySettings.ToDictionary(t => t.EntityTypeFullName);

            IDictionary<string, EntitySetting> resusltEntitySchemaDict = new Dictionary<string, EntitySetting>();

            allEntityTypes.ForEach(type =>
            {
                DatabaseEntityAttribute attribute = type.GetCustomAttribute<DatabaseEntityAttribute>();

                fileConfiguredDict.TryGetValue(type.FullName, out EntitySetting fileConfigured);

                EntitySetting entitySchema = new EntitySetting
                {
                    EntityTypeFullName = type.FullName
                };

                if (attribute != null)
                {
                    entitySchema.DatabaseName = attribute.DatabaseName.IsNullOrEmpty() ? _databaseEngine.FirstDefaultDatabaseName : attribute.DatabaseName!;

                    if (attribute.TableName.IsNullOrEmpty())
                    {
                        entitySchema.TableName = "tb_";

                        if (type.Name.EndsWith(attribute.SuffixToRemove, GlobalSettings.Comparison))
                        {
                            entitySchema.TableName += type.Name.Substring(0, type.Name.Length - attribute.SuffixToRemove.Length).ToLower(GlobalSettings.Culture);
                        }
                        else
                        {
                            entitySchema.TableName += type.Name.ToLower(GlobalSettings.Culture);
                        }
                    }
                    else
                    {
                        entitySchema.TableName = attribute.TableName!;
                    }

                    entitySchema.Description = attribute.Description;
                    entitySchema.ReadOnly = attribute.ReadOnly;
                }

                //文件配置可以覆盖代码中的配置
                if (fileConfigured != null)
                {
                    if (!string.IsNullOrEmpty(fileConfigured.DatabaseName))
                    {
                        entitySchema.DatabaseName = fileConfigured.DatabaseName;
                    }

                    if (!string.IsNullOrEmpty(fileConfigured.TableName))
                    {
                        entitySchema.TableName = fileConfigured.TableName;
                    }

                    if (!string.IsNullOrEmpty(fileConfigured.Description))
                    {
                        entitySchema.Description = fileConfigured.Description;
                    }

                    entitySchema.ReadOnly = fileConfigured.ReadOnly;
                }

                //做最后的检查，有可能两者都没有定义
                if (entitySchema.DatabaseName.IsNullOrEmpty())
                {
                    entitySchema.DatabaseName = _databaseEngine.FirstDefaultDatabaseName;
                }

                if (entitySchema.TableName.IsNullOrEmpty())
                {
                    entitySchema.TableName = "tb_" + type.Name.ToLower(GlobalSettings.Culture);
                }

                resusltEntitySchemaDict.Add(type.FullName, entitySchema);
            });

            return resusltEntitySchemaDict;
        }

        public DatabaseEntityDef GetDef<T>() where T : Entity
        {
            return GetDef(typeof(T));
        }

        public DatabaseEntityDef GetDef(Type entityType)
        {
            if (!_defDict.ContainsKey(entityType))
            {
                lock (_lockObj)
                {
                    if (!_defDict.ContainsKey(entityType))
                    {
                        _defDict[entityType] = CreateEntityDef(entityType);
                    }
                }
            }

            return _defDict[entityType];
        }

        private DatabaseEntityDef CreateEntityDef(Type entityType)
        {
            if (!_entitySchemaDict.TryGetValue(entityType.FullName, out EntitySetting dbSchema))
            {
                throw new DatabaseException($"Type不是Entity，或者没有DatabaseEntityAttribute. Type:{entityType}");
            }

            DatabaseEntityDef entityDef = new DatabaseEntityDef();


            entityDef.EntityType = entityType;
            entityDef.EntityFullName = entityType.FullName;
            entityDef.DatabaseName = dbSchema.DatabaseName;
            entityDef.TableName = dbSchema.TableName;
            entityDef.DbTableReservedName = _databaseEngine.GetReservedStatement(entityDef.TableName!);
            entityDef.DatabaseWriteable = !dbSchema.ReadOnly;



            foreach (PropertyInfo info in entityType.GetProperties())
            {
                EntityPropertyAttribute entityPropertyAttribute = info.GetCustomAttribute<EntityPropertyAttribute>(true);

                if (entityPropertyAttribute == null)
                {
                    continue;
                }

                DatabaseEntityPropertyDef propertyDef = CreatePropertyDef(entityDef, info, entityPropertyAttribute);

                entityDef.FieldCount++;

                if (propertyDef.IsUnique)
                {
                    entityDef.UniqueFieldCount++;
                }

                entityDef.PropertyDefs.Add(propertyDef);
                entityDef.PropertyDict.Add(propertyDef.Name, propertyDef);
            }

            return entityDef;
        }

        private DatabaseEntityPropertyDef CreatePropertyDef(DatabaseEntityDef entityDef, PropertyInfo propertyInfo, EntityPropertyAttribute propertyAttribute)
        {
            DatabaseEntityPropertyDef propertyDef = new DatabaseEntityPropertyDef();

            propertyDef.EntityDef = entityDef;
            propertyDef.Name = propertyInfo.Name;
            propertyDef.Type = propertyInfo.PropertyType;
            propertyDef.NullableUnderlyingType = Nullable.GetUnderlyingType(propertyDef.Type);
            propertyDef.SetMethod = ReflectUtil.GetPropertySetterMethod(propertyInfo, entityDef.EntityType);
            propertyDef.GetMethod = ReflectUtil.GetPropertySetterMethod(propertyInfo, entityDef.EntityType);


            propertyDef.IsNullable = !propertyAttribute.NotNull;
            propertyDef.IsUnique = propertyAttribute.Unique;
            propertyDef.DbMaxLength = propertyAttribute.MaxLength > 0 ? (int?)propertyAttribute.MaxLength : null;
            propertyDef.IsLengthFixed = propertyAttribute.FixedLength;

            propertyDef.DbReservedName = _databaseEngine.GetReservedStatement(propertyDef.Name);
            propertyDef.DbParameterizedName = _databaseEngine.GetParameterizedStatement(propertyDef.Name);

            if (propertyAttribute.Converter != null)
            {
                propertyDef.TypeConverter = _typeConverterFactory.GetTypeConverter(propertyAttribute.Converter);
            }


            //判断是否是主键
            AutoIncrementPrimaryKeyAttribute? atts1 = propertyInfo.GetCustomAttribute<AutoIncrementPrimaryKeyAttribute>(false);

            if (atts1 != null)
            {
                propertyDef.IsAutoIncrementPrimaryKey = true;
                propertyDef.IsNullable = false;
                propertyDef.IsForeignKey = false;
                propertyDef.IsUnique = true;
            }
            else
            {
                //判断是否外键
                ForeignKeyAttribute atts2 = propertyInfo.GetCustomAttribute<ForeignKeyAttribute>(false);

                if (atts2 != null)
                {
                    propertyDef.IsAutoIncrementPrimaryKey = false;
                    propertyDef.IsForeignKey = true;
                    propertyDef.IsNullable = true;
                    propertyDef.IsUnique = false;
                }
            }

            return propertyDef;
        }

        public int GetVarcharDefaultLength()
        {
            return _databaseSettings.DefaultVarcharLength == 0 ? DEFAULT_VARCHAR_LENGTH : _databaseSettings.DefaultVarcharLength;
        }

        public IEnumerable<DatabaseEntityDef> GetAllDefsByDatabase(string databaseName)
        {
            return _defDict.Values.Where(def => databaseName.Equals(def.DatabaseName, GlobalSettings.ComparisonIgnoreCase));
        }
    }
}
