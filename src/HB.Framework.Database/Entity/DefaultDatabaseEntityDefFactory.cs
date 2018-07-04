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
        private readonly int DEFAULT_STRING_LENGTH = 100;

        private ConcurrentDictionary<Type, DatabaseEntityDef> _defDict;
        private readonly object _lockObj;
        private DatabaseOptions _options;
        private IDatabaseEngine _databaseEngine;

        public DefaultDatabaseEntityDefFactory(IOptions<DatabaseOptions> options, IDatabaseEngine databaseEngine)
        {
            _options = options.Value;
            _databaseEngine = databaseEngine;
            _defDict = new ConcurrentDictionary<Type, DatabaseEntityDef>();
            _lockObj = new object();
        }

        public DatabaseEntityDef Get<T>()
        {
            return Get(typeof(T));
        }

        public DatabaseEntityDef Get(Type domainType)
        {
            if (!_defDict.ContainsKey(domainType))
            {
                lock (_lockObj)
                {
                    if (!_defDict.ContainsKey(domainType))
                    {
                        _defDict[domainType] = CreateModelDef(domainType);
                    }
                }
            }

            return _defDict[domainType];
        }

        private DatabaseEntityDef CreateModelDef(Type modelType)
        {            
            DatabaseEntityDef modelDef = new DatabaseEntityDef();

            #region 自身

            modelDef.EntityType = modelType;
            modelDef.EntityFullName = modelType.FullName;
            modelDef.PropertyDict = new Dictionary<string, DatabaseEntityPropertyDef>();

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
                IEnumerable<Attribute> atts2 = info.GetCustomAttributes(typeof(DatabaseEntityPropertyIgnoreAttribute), false).Select<object, Attribute>(o=>(Attribute)o);

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

            //判断是否是主键
            IEnumerable<Attribute> atts1 = info.GetCustomAttributes(typeof(DatabaseMainKeyAttribute), false).Select<object, Attribute>(o => (Attribute)o);
            if (atts1 != null && atts1.Count() > 0)
            {
                propertyDef.IsTableProperty = true;
                propertyDef.IsPrimaryKey = true;
                propertyDef.IsNullable = false;
                propertyDef.IsForeignKey = false;
                propertyDef.IsUnique = false;
                propertyDef.DbDescription = (atts1.ElementAt(0) as DatabaseMainKeyAttribute).Description;
                propertyDef.DbDefaultValue = null;
                propertyDef.DbLength = null;
            }
            else
            {
                //判断是否外键
                IEnumerable<Attribute> atts2 = info.GetCustomAttributes(typeof(DatabaseForeignKeyAttribute), false).Select<object, Attribute>(o => (Attribute)o);
                if (atts2 != null && atts2.Count() > 0)
                {
                    propertyDef.IsTableProperty = true;
                    propertyDef.IsPrimaryKey = false;
                    propertyDef.IsForeignKey = true;
                    propertyDef.IsNullable = false;
                    propertyDef.IsUnique = false;
                    propertyDef.DbDescription = (atts2.ElementAt(0) as DatabaseForeignKeyAttribute).Description;
                    propertyDef.DbDefaultValue = null;
                    propertyDef.DbLength = null;
                }
                else
                {
                    //判断是否TableProperty
                    IEnumerable<Attribute> atts3 = info.GetCustomAttributes(typeof(DatabaseEntityPropertyAttribute), false).Select<object, Attribute>(o => (Attribute)o);
                    if (atts3 != null && atts3.Count() > 0)
                    {
                        var cur = atts3.ElementAt(0) as DatabaseEntityPropertyAttribute;

                        propertyDef.IsTableProperty = true;
                        propertyDef.IsPrimaryKey = false;
                        propertyDef.IsForeignKey = false;
                        propertyDef.IsNullable = !cur.NotNull;
                        propertyDef.IsUnique = cur.Unique;
                        propertyDef.DbLength = cur.Length > 0 ? (Nullable<int>)cur.Length : null;
                        propertyDef.DbDefaultValue = string.IsNullOrEmpty(cur.DefaultValue) ? null : cur.DefaultValue;
                        propertyDef.DbDescription = cur.Description;
                    }
                }
            }

            if (propertyDef.IsTableProperty)
            {
                propertyDef.DbReservedName = _databaseEngine.GetReservedStatement(propertyDef.PropertyName);
                propertyDef.DbParameterizedName = _databaseEngine.GetParameterizedStatement(propertyDef.PropertyName);
                propertyDef.DbFieldType = _databaseEngine.GetDbType(propertyDef.PropertyType);
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
