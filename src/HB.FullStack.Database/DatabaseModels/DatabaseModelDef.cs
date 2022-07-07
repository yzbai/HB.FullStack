using System;
using System.Collections.Generic;


namespace HB.FullStack.Database.DatabaseModels
{
    /// <summary>
    /// 实体定义
    /// </summary>
    public class DatabaseModelDef
    {
        /// <summary>
        /// 是否是GuidModel
        /// </summary>
        public bool IsIdGuid { get; set; }

        /// <summary>
        /// 是否是IdLongModel
        /// </summary>
        public bool IsIdLong { get; set; }

        /// <summary>
        /// 是否是AutoincrementIdModel
        /// </summary>
        public bool IsIdAutoIncrement { get; set; }

        /// <summary>
        /// 实体名
        /// </summary>
        public string ModelFullName { get; set; } = null!;

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type ModelType { get; set; } = null!;

        /// <summary>
        /// 所属数据库
        /// 在IsTableModel为false时没有意义
        /// </summary>
        public string DatabaseName { get; set; } = null!;

        /// <summary>
        /// 数据库是否可写
        /// </summary>
        public bool DatabaseWriteable { get; set; }

        /// <summary>
        /// 数据库表名
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string TableName { get; set; } = null!;

        /// <summary>
        /// 数据库专有化后的名称
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string DbTableReservedName { get; set; } = null!;

        public int FieldCount { get; set; }

        public int UniqueFieldCount { get; set; }

        /// <summary>
        /// 属性字典
        /// </summary>
        public Dictionary<string, DatabaseModelPropertyDef> PropertyDict { get; } = new Dictionary<string, DatabaseModelPropertyDef>();

        /// <summary>
        /// 属性枚举
        /// </summary>
        public IList<DatabaseModelPropertyDef> PropertyDefs { get; } = new List<DatabaseModelPropertyDef>();

        public DatabaseModelPropertyDef PrimaryKeyPropertyDef { get; internal set; } = null!;

        public DatabaseModelPropertyDef? GetPropertyDef(string propertyName)
        {
            if (PropertyDict.TryGetValue(propertyName, out DatabaseModelPropertyDef? propertyDef))
            {
                return propertyDef;
            }

            return null;

            //if (PropertyDict.ContainsKey(propertyName))
            //{
            //    return PropertyDict[propertyName];
            //}

            //return null;
        }

        public bool ContainsProperty(string propertyName)
        {
            return PropertyDict.ContainsKey(propertyName);
        }
    }
}