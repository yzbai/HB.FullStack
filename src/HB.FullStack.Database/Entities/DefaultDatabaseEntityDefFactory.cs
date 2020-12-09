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
        private readonly IDatabaseTypeConverterFactory _typeConverterFactory;

        private readonly IDictionary<string, EntityInfo> _entitySchemaDict;
        private readonly IDictionary<Type, DatabaseEntityDef> _defDict = new Dictionary<Type, DatabaseEntityDef>();

        public DefaultDatabaseEntityDefFactory(IDatabaseEngine databaseEngine, IDatabaseTypeConverterFactory typeConverterFactory)
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

        private IDictionary<string, EntityInfo> ConstructeSchemaDict(IEnumerable<Type> allEntityTypes)
        {
            IDictionary<string, EntityInfo> fileConfiguredDict = _databaseSettings.Entities.ToDictionary(t => t.EntityTypeFullName);

            IDictionary<string, EntityInfo> resusltEntitySchemaDict = new Dictionary<string, EntityInfo>();

            allEntityTypes.ForEach(type =>
            {

                DatabaseEntityAttribute attribute = type.GetCustomAttribute<DatabaseEntityAttribute>();

                fileConfiguredDict.TryGetValue(type.FullName, out EntityInfo fileConfigured);

                EntityInfo entitySchema = new EntityInfo
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

        public DatabaseEntityDef GetDef<T>()
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

        /// <summary>
        /// CreateEntityDef
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <exception cref="TypeLoadException">Ignore.</exception>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        private DatabaseEntityDef CreateEntityDef(Type entityType)
        {
            DatabaseEntityDef entityDef = new DatabaseEntityDef(entityType);

            #region 数据库

            if (_entitySchemaDict.TryGetValue(entityType.FullName, out EntityInfo dbSchema))
            {
                entityDef.IsTableModel = true;
                entityDef.DatabaseName = dbSchema.DatabaseName;
                entityDef.TableName = dbSchema.TableName;
                entityDef.DbTableDescription = dbSchema.Description;
                entityDef.DbTableReservedName = _databaseEngine.GetReservedStatement(entityDef.TableName!);
                entityDef.DatabaseWriteable = !dbSchema.ReadOnly;
            }
            else
            {
                entityDef.IsTableModel = false;
            }

            #endregion

            #region 属性

            foreach (PropertyInfo info in entityType.GetProperties())
            {
                IEnumerable<Attribute> atts2 = info.GetCustomAttributes(typeof(EntityPropertyIgnoreAttribute), false).Select(o => (Attribute)o);

                if (atts2.IsNullOrEmpty())
                {
                    DatabaseEntityPropertyDef propertyDef = CreatePropertyDef(entityDef, info);

                    entityDef.FieldCount++;

                    if (propertyDef.IsUnique)
                    {
                        entityDef.UniqueFieldCount++;
                    }

                    entityDef.Properties.Add(propertyDef);
                    entityDef.PropertyDict.Add(propertyDef.PropertyInfo.Name, propertyDef);
                }
            }

            #endregion

            return entityDef;
        }

        /// <summary>
        /// CreatePropertyDef
        /// </summary>
        /// <param name="entityDef"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <exception cref="TypeLoadException">Ignore.</exception>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        private DatabaseEntityPropertyDef CreatePropertyDef(DatabaseEntityDef entityDef, PropertyInfo info)
        {
            DatabaseEntityPropertyDef propertyDef = new DatabaseEntityPropertyDef(entityDef, info);

            #region 数据库

            IEnumerable<Attribute> propertyAttrs = info.GetCustomAttributes(typeof(EntityPropertyAttribute), false).Select(o => (Attribute)o);

            if (propertyAttrs.IsNotNullOrEmpty())
            {
                if (propertyAttrs.ElementAt(0) is EntityPropertyAttribute propertyAttr)
                {
                    propertyDef.IsTableProperty = true;
                    propertyDef.IsNullable = !propertyAttr.NotNull;
                    propertyDef.IsUnique = propertyAttr.Unique;
                    propertyDef.DbMaxLength = propertyAttr.Length > 0 ? (int?)propertyAttr.Length : null;
                    propertyDef.IsLengthFixed = propertyAttr.FixedLength;
                    propertyDef.DbDefaultValue = ValueConverterUtil.TypeValueToStringValue(propertyAttr.DefaultValue);
                    propertyDef.DbDescription = propertyAttr.Description;

                    if (propertyAttr.Converter != null)
                    {
                        propertyDef.TypeConverter = _typeConverterFactory.GetTypeConverter(propertyAttr.Converter);
                    }
                }
            }

            //判断是否是主键
            IEnumerable<Attribute> atts1 = info.GetCustomAttributes(typeof(AutoIncrementPrimaryKeyAttribute), false).Select(o => (Attribute)o);

            if (atts1.IsNotNullOrEmpty())
            {
                propertyDef.IsTableProperty = true;
                propertyDef.IsAutoIncrementPrimaryKey = true;
                propertyDef.IsNullable = false;
                propertyDef.IsForeignKey = false;
                propertyDef.IsUnique = true;
            }
            else
            {
                //判断是否外键
                IEnumerable<Attribute> atts2 = info.GetCustomAttributes(typeof(ForeignKeyAttribute), false).Select(o => (Attribute)o);

                if (atts2.IsNotNullOrEmpty())
                {
                    propertyDef.IsTableProperty = true;
                    propertyDef.IsAutoIncrementPrimaryKey = false;
                    propertyDef.IsForeignKey = true;
                    propertyDef.IsNullable = true;
                    propertyDef.IsUnique = false;
                }
            }

            if (propertyDef.IsTableProperty)
            {
                propertyDef.DbReservedName = _databaseEngine.GetReservedStatement(propertyDef.PropertyInfo.Name);
                propertyDef.DbParameterizedName = _databaseEngine.GetParameterizedStatement(propertyDef.PropertyInfo.Name);

                if (propertyDef.TypeConverter != null)
                {
                    propertyDef.DbFieldType = propertyDef.TypeConverter.TypeToDbType(propertyDef.PropertyInfo.PropertyType);
                }
                else
                {
                    propertyDef.DbFieldType = _databaseEngine.GetDbType(propertyDef.PropertyInfo.PropertyType);
                }
            }

            #endregion

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
