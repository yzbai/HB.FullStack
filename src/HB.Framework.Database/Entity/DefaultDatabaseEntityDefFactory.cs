using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using HB.Framework.Database.Engine;
using System.Linq;
using HB.Framework.Common.Entity;
using System.IO;
using HB.Framework.Common.Utility;

namespace HB.Framework.Database.Entity
{
    /// <summary>
    /// 实体定义集合
    /// 多线程公用
    /// 单例
    /// </summary>
    internal class DefaultDatabaseEntityDefFactory : IDatabaseEntityDefFactory
    {
        private readonly int DEFAULT_STRING_LENGTH = 200;

        private readonly object _lockObj = new object();
        private readonly IDatabaseSettings _databaseSettings;
        private readonly IDatabaseEngine _databaseEngine;
        private readonly IDatabaseTypeConverterFactory _typeConverterFactory;

        private readonly IEnumerable<Type> _allEntityTypes;
        private readonly IDictionary<string, EntitySchema> _entitySchemaDict;
        private readonly IDictionary<Type, DatabaseEntityDef> _defDict = new Dictionary<Type, DatabaseEntityDef>();

        public DefaultDatabaseEntityDefFactory(IDatabaseSettings databaseSettings, IDatabaseEngine databaseEngine, IDatabaseTypeConverterFactory typeConverterFactory)
        {
            _databaseSettings = databaseSettings;
            _databaseEngine = databaseEngine;
            _typeConverterFactory = typeConverterFactory;

            _allEntityTypes = ReflectUtil.GetAllTypeByCondition(t => t.IsSubclassOf(typeof(DatabaseEntity)));
            _entitySchemaDict = ConstructeSchemaDict();

            WarmUp();
        }

        private void WarmUp()
        {
            _allEntityTypes.ForEach(t => _defDict[t] = CreateModelDef(t));
        }

        private IDictionary<string, EntitySchema> ConstructeSchemaDict()
        {
            IDictionary<string, EntitySchema> fileConfiguredDict = _databaseSettings.Entities.ToDictionary(t => t.EntityTypeFullName);

            IDictionary<string, EntitySchema> resusltEntitySchemaDict = new Dictionary<string, EntitySchema>();

            _allEntityTypes.ForEach(type => {

                EntitySchemaAttribute attribute = type.GetCustomAttribute<EntitySchemaAttribute>();

                fileConfiguredDict.TryGetValue(type.FullName, out EntitySchema fileConfigured);

                if (attribute == null && fileConfigured == null)
                {
                    throw new Exception($"DatabaseEntity : {type.FullName} not configure neither appsettings.json or EntitySchemaAttribute.");
                }

                EntitySchema entitySchema = new EntitySchema();

                if (attribute != null)
                {
                    entitySchema.EntityTypeFullName = type.FullName;
                    entitySchema.DatabaseName = attribute.DatabaseName;
                    entitySchema.TableName = attribute.TableName;
                    entitySchema.Description = attribute.Description;
                    entitySchema.Writeable = attribute.Writeable;
                }

                //文件配置可以覆盖代码中的配置
                if (fileConfigured != null)
                {
                    entitySchema.EntityTypeFullName = type.FullName;

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

                    entitySchema.Writeable = fileConfigured.Writeable;
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
                        _defDict[entityType] = CreateModelDef(entityType);
                    }
                }
            }

            return _defDict[entityType];
        }

        private DatabaseEntityDef CreateModelDef(Type modelType)
        {
            DatabaseEntityDef modelDef = new DatabaseEntityDef();

            #region 自身

            modelDef.EntityType = modelType;
            modelDef.EntityFullName = modelType.FullName;
            //modelDef.PropertyDict = new Dictionary<string, DatabaseEntityPropertyDef>();

            #endregion

            #region 数据库

            if (_entitySchemaDict.TryGetValue(modelType.FullName, out EntitySchema dbSchema))
            {
                modelDef.IsTableModel = true;
                modelDef.DatabaseName = dbSchema.DatabaseName;
                modelDef.TableName = dbSchema.TableName.IsNullOrEmpty() ? "tb_" + modelType.Name.ToLower() : dbSchema.TableName;
                modelDef.DbTableDescription = dbSchema.Description;
                modelDef.DbTableReservedName = _databaseEngine.GetReservedStatement(modelDef.TableName);
                modelDef.DatabaseWriteable = dbSchema.Writeable;
            }
            else
            {
                modelDef.IsTableModel = false;
            }

            #endregion

            #region 属性

            foreach (PropertyInfo info in modelType.GetTypeInfo().GetProperties())
            {
                IEnumerable<Attribute> atts2 = info.GetCustomAttributes(typeof(EntityPropertyIgnoreAttribute), false).Select<object, Attribute>(o => (Attribute)o);

                if (atts2 == null || atts2.Count() == 0)
                {
                    DatabaseEntityPropertyDef propertyDef = CreatePropertyDef(modelDef, info);

                    modelDef.PropertyDict.Add(propertyDef.PropertyName, propertyDef);

                    modelDef.FieldCount++;
                }
            }

            #endregion

            return modelDef;
        }

        private DatabaseEntityPropertyDef CreatePropertyDef(DatabaseEntityDef modelDef, PropertyInfo info)
        {
            DatabaseEntityPropertyDef propertyDef = new DatabaseEntityPropertyDef();

            #region 自身

            propertyDef.EntityDef = modelDef;
            propertyDef.PropertyName = info.Name;
            propertyDef.PropertyType = info.PropertyType;
            propertyDef.GetMethod = info.GetGetMethod();
            propertyDef.SetMethod = info.GetSetMethod();

            #endregion

            #region 数据库

            IEnumerable<Attribute> propertyAttrs = info.GetCustomAttributes(typeof(EntityPropertyAttribute), false).Select(o => (Attribute)o);
            if (propertyAttrs != null && propertyAttrs.Count() > 0)
            {
                EntityPropertyAttribute propertyAttr = propertyAttrs.ElementAt(0) as EntityPropertyAttribute;

                propertyDef.IsTableProperty = true;
                propertyDef.IsNullable = !propertyAttr.NotNull;
                propertyDef.IsUnique = propertyAttr.Unique;
                propertyDef.DbLength = propertyAttr.Length > 0 ? (int?)propertyAttr.Length : null;
                propertyDef.IsLengthFixed = propertyAttr.FixedLength;
                propertyDef.DbDefaultValue = ValueConverter.TypeValueToDbValue(propertyAttr.DefaultValue);
                propertyDef.DbDescription = propertyAttr.Description;

                if (propertyAttr.ConverterType != null)
                {
                    propertyDef.TypeConverter = _typeConverterFactory.GetTypeConverter(propertyAttr.ConverterType);
                }
            }

            //判断是否是主键
            IEnumerable<Attribute> atts1 = info.GetCustomAttributes(typeof(AutoIncrementPrimaryKeyAttribute), false).Select<object, Attribute>(o => (Attribute)o);
            if (atts1 != null && atts1.Count() > 0)
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
                IEnumerable<Attribute> atts2 = info.GetCustomAttributes(typeof(ForeignKeyAttribute), false).Select<object, Attribute>(o => (Attribute)o);
                if (atts2 != null && atts2.Count() > 0)
                {
                    propertyDef.IsTableProperty = true;
                    propertyDef.IsAutoIncrementPrimaryKey = false;
                    propertyDef.IsForeignKey = true;
                    //propertyDef.IsNullable = false;
                    propertyDef.IsUnique = false;
                }
            }

            if (propertyDef.IsTableProperty)
            {
                propertyDef.DbReservedName = _databaseEngine.GetReservedStatement(propertyDef.PropertyName);
                propertyDef.DbParameterizedName = _databaseEngine.GetParameterizedStatement(propertyDef.PropertyName);

                if (propertyDef.TypeConverter != null)
                {
                    propertyDef.DbFieldType = propertyDef.TypeConverter.TypeToDbType(propertyDef.PropertyType);
                }
                else
                {
                    propertyDef.DbFieldType = _databaseEngine.GetDbType(propertyDef.PropertyType);
                }
            }

            #endregion

            return propertyDef;
        }

        public int GetVarcharDefaultLength()
        {
            return _databaseSettings.DefaultVarcharLength == 0 ? DEFAULT_STRING_LENGTH : _databaseSettings.DefaultVarcharLength;
        }

        public IEnumerable<DatabaseEntityDef> GetAllDefsByDatabase(string databaseName)
        {
            return _defDict.Values.Where(def => def.DatabaseName.Equals(databaseName, GlobalSettings.ComparisonIgnoreCase));
        }
    }

}
