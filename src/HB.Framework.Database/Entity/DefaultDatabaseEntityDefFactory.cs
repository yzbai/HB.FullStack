using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Options;
using HB.Framework.Database.Engine;
using System.Linq;

namespace HB.Framework.Database.Entity
{
    /// <summary>
    /// 实体定义集合
    /// 多线程公用
    /// 单例
    /// </summary>
    public class DefaultDatabaseEntityDefFactory : IDatabaseEntityDefFactory
    {
        private readonly int DEFAULT_STRING_LENGTH = 200;

        private readonly ConcurrentDictionary<Type, DatabaseEntityDef> _defDict;
        private readonly object _lockObj;
        private readonly DatabaseOptions _options;
        private readonly IDatabaseEngine _databaseEngine;
        private readonly IDatabaseTypeConverterFactory typeConverterFactory;

        public DefaultDatabaseEntityDefFactory(IOptions<DatabaseOptions> options, IDatabaseEngine databaseEngine, IDatabaseTypeConverterFactory typeConverterFactory)
        {
            _options = options.Value;
            _databaseEngine = databaseEngine;
            _defDict = new ConcurrentDictionary<Type, DatabaseEntityDef>();
            _lockObj = new object();
            this.typeConverterFactory = typeConverterFactory;
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

            DatabaseSchema dbSchema = _options.GetDatabaseSchema(modelType.FullName);

            if (dbSchema == null)
            {
                modelDef.IsTableModel = false;
            }
            else
            {
                modelDef.IsTableModel = true;
                modelDef.DatabaseName = dbSchema.DatabaseName;
                modelDef.TableName = dbSchema.TableName;
                modelDef.DbTableDescription = dbSchema.Description;
                modelDef.DbTableReservedName = _databaseEngine.GetReservedStatement(modelDef.TableName);
                modelDef.DatabaseWriteable = dbSchema.Writeable;
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
                propertyDef.DbDefaultValue = string.IsNullOrEmpty(propertyAttr.DefaultValue) ? null : propertyAttr.DefaultValue;
                propertyDef.DbDescription = propertyAttr.Description;

                if (propertyAttr.ConverterType != null)
                {
                    propertyDef.TypeConverter = typeConverterFactory.GetTypeConverter(propertyAttr.ConverterType);
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
                    propertyDef.IsNullable = false;
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
            return _options.DefaultVarcharLength == 0 ? DEFAULT_STRING_LENGTH : _options.DefaultVarcharLength;
        }
    }

}
